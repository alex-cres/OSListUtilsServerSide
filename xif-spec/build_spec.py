"""Generate the Integration Studio spec + sources for the ListUtils O11 extension (.xif).

Inputs (single source of truth, never edited here):
  ListUtils.O11/IssListUtils.cs          signatures (32 actions)
  ListUtils/IListUtils.cs, Condition.cs  action / parameter / Condition field descriptions
  ListUtils.O11/Actions/*.cs, Condition.cs, Operators.cs   the O11 logic
  ListUtils.O11/bin/Release/net48/*.dll  System.Text.Json and its dependencies

Outputs (next to this script):
  spec.json   for .github/skills/integrationstudio-ui-automation
  src/        ListUtils.cs (adapter: IS-generated types -> core) + the O11 partials renamed
              CssListUtils -> ListUtilsCore, so they don't clash with the IS-generated interface.

Integration Studio has no List-of-Text parameter type: List<string> becomes a Record List of
the TextItem structure (one Text attribute, Value).

Usage:  dotnet build ..\\ListUtils.O11 -c Release ; python build_spec.py
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parent
O11 = ROOT / "ListUtils.O11"
ODC = ROOT / "ListUtils"
BIN = O11 / "bin" / "Release" / "net48"
SRC = HERE / "src"
NAME = "ListUtils"

TYPE_MAP = {"string": ("Text", None), "int": ("Integer", None), "bool": ("Boolean", None),
            "List<Condition>": ("RecordList", "Condition"), "List<string>": ("RecordList", "TextItem")}
IS_TYPES = {"List<Condition>": "RLConditionRecordList", "List<string>": "RLTextItemRecordList"}
# Inputs that carry the data being processed; all other inputs are optional (empty/0/False defaults).
MANDATORY = re.compile(r"^(SourceListJson|ListAJson|ListBJson|ListsJson|ChunksListJson)$")


def unescape(s: str) -> str:
    return s.replace('\\"', '"').replace("\\\\", "\\")


def parse_o11_interface() -> list[dict]:
    text = (O11 / "IssListUtils.cs").read_text(encoding="utf-8-sig")
    actions = []
    for m in re.finditer(r"void\s+Mss(\w+)\s*\(([^)]*)\)\s*;", text):
        params = []
        for p in m.group(2).split(","):
            pm = re.match(r"\s*(out\s+)?([\w<>]+)\s+ss(\w+)\s*$", p)
            if not pm:
                sys.exit(f"cannot parse parameter {p!r} of Mss{m.group(1)}")
            params.append({"out": bool(pm.group(1)), "cs": pm.group(2), "name": pm.group(3)})
        actions.append({"name": m.group(1), "params": params})
    return actions


def parse_odc_descriptions() -> tuple[str, dict, dict, str, dict]:
    text = (ODC / "IListUtils.cs").read_text(encoding="utf-8-sig")
    desc = r'Description\s*=\s*"((?:[^"\\]|\\.)*)"'
    ext = unescape(re.search(r"\[OSInterface\(" + desc, text).group(1))
    actions, params = {}, {}
    for chunk in text.split("[OSAction(")[1:]:
        m = re.match(desc + r"[^\]]*\)\]\s*void\s+(\w+)\s*\((.*)", chunk, re.S)
        if not m:
            continue
        actions[m.group(2)] = unescape(m.group(1))
        for pm in re.finditer(r"\[OSParameter\(" + desc + r"[^\]]*\)\]\s*(?:out\s+)?[\w<>]+\s+(\w+)", m.group(3)):
            params[(m.group(2), pm.group(2))] = unescape(pm.group(1))
    cond = (ODC / "Condition.cs").read_text(encoding="utf-8-sig")
    struct_desc = unescape(re.search(r"\[OSStructure\(" + desc, cond).group(1))
    fields = {f: unescape(d) for d, f in re.findall(r"\[OSStructureField\(" + desc + r"[^\]]*\)\]\s*public\s+\w+\s+(\w+)", cond)}
    return ext, actions, params, struct_desc, fields


def readme_descriptions() -> dict:
    """Fallback: README tables (### List_X, | `Param` | type | dir | description |)."""
    out, action = {}, None
    for line in (ROOT / "README.md").read_text(encoding="utf-8").splitlines():
        h = re.match(r"###\s+(List_\w+)", line)
        if h:
            action = h.group(1)
            continue
        r = re.match(r"\|\s*`(\w+)`\s*\|[^|]*\|[^|]*\|\s*(.*?)\s*\|\s*$", line)
        if action and r:
            out[(action, r.group(1))] = r.group(2).replace("`", "")
    return out


def build_spec(actions: list[dict]) -> dict:
    ext_desc, act_desc, par_desc, cond_desc, cond_fields = parse_odc_descriptions()
    readme = readme_descriptions()
    missing, spec_actions = [], []
    for a in actions:
        if a["name"] not in act_desc:
            missing.append(a["name"])
        ins, outs = [], []
        for p in a["params"]:
            t, rec = TYPE_MAP[p["cs"]]
            d = par_desc.get((a["name"], p["name"])) or readme.get((a["name"], p["name"]), "")
            if not d:
                missing.append(f"{a['name']}.{p['name']}")
            item = {"name": p["name"], "type": t, "description": d}
            if rec:
                item["record"] = rec
            if not p["out"]:
                item["mandatory"] = bool(MANDATORY.match(p["name"]))
            (outs if p["out"] else ins).append(item)
        spec_actions.append({"name": a["name"], "description": act_desc.get(a["name"], ""), "inputs": ins, "outputs": outs})
    if missing:
        sys.exit(f"missing descriptions: {missing}")

    def field(n, t, length=None):
        f = {"name": n, "type": t, "mandatory": False, "description": cond_fields[n]}
        if length:
            f["length"] = length
        return f

    sources = {f"{NAME}.cs": "src/ListUtils.cs"}
    for f in sorted(SRC.glob("*.cs")):
        if f.name != "ListUtils.cs":
            sources[f.name] = f"src/{f.name}"
    binaries = sorted(f"../ListUtils.O11/bin/Release/net48/{d.name}" for d in BIN.glob("*.dll") if d.name != "ListUtils.dll")
    return {
        "name": NAME,
        "description": ext_desc,
        "icon": "../ListUtils.O11/resources/icon.ico",
        "structures": [
            {"name": "Condition", "description": cond_desc,
             "attributes": [field("Path", "Text", 2000), field("Operator", "Text", 50),
                            field("Value", "Text", 2000), field("CaseSensitive", "Boolean")]},
            {"name": "TextItem",
             "description": "One Text value. Integration Studio has no List-of-Text parameter type, so Text lists "
                            "(property paths, key names, JSON chunks) are passed as a Record List of TextItem.",
             "attributes": [{"name": "Value", "type": "Text", "length": 2000, "mandatory": False,
                             "description": "The text value."}]},
        ],
        "actions": spec_actions,
        "sources": sources,
        "binaries": binaries,
        "csproj_properties": {"TargetFrameworkVersion": "v4.8", "LangVersion": "10", "Nullable": "enable"},
    }


def write_core_sources() -> list[str]:
    """O11 partials -> ListUtilsCore (internal); the interface shell ListUtilsActions.cs is replaced by the adapter."""
    SRC.mkdir(exist_ok=True)
    for old in SRC.glob("*.cs"):
        old.unlink()
    written = []
    files = [(O11 / "Condition.cs", "ListUtils.Condition.cs"), (O11 / "Operators.cs", "ListUtils.Operators.cs")]
    for f in sorted((O11 / "Actions").glob("ListUtilsActions.*.cs")):
        files.append((f, f.name.replace("ListUtilsActions.", "ListUtils.Core.")))
    for src, dst in files:
        text = src.read_text(encoding="utf-8-sig")
        if "IssListUtils" in text:
            sys.exit(f"{src.name} references IssListUtils; the adapter replaces the interface")
        text = re.sub(r"\bpublic\s+partial\s+class\s+CssListUtils\b", "internal partial class ListUtilsCore", text)
        if "CssListUtils" in text:
            sys.exit(f"{src.name}: unexpected CssListUtils reference")
        (SRC / dst).write_bytes(text.replace("\r\n", "\n").replace("\n", "\r\n").encode("utf-8"))
        written.append(dst)
    return written


def write_adapter(actions: list[dict]) -> None:
    lines = [
        "using System;",
        "using System.Collections;",
        "using System.Collections.Generic;",
        "using System.Data;",
        "using OutSystems.HubEdition.RuntimePlatform;",
        "using OutSystems.RuntimePublic.Db;",
        "",
        "namespace OutSystems.NssListUtils {",
        "",
        "\t// Generated by xif-spec/build_spec.py. Implements the Integration Studio interface by converting",
        "\t// RLConditionRecordList / RLTextItemRecordList to the List<> types of ListUtilsCore (the O11 logic).",
        "\tpublic class CssListUtils: IssListUtils {",
        "",
        "\t\tprivate readonly ListUtilsCore _core = new ListUtilsCore();",
    ]
    for a in actions:
        sig, call, post = [], [], []
        for p in a["params"]:
            n = "ss" + p["name"]
            sig.append(("out " if p["out"] else "") + f"{IS_TYPES.get(p['cs'], p['cs'])} {n}")
            if p["out"] and p["cs"] == "List<string>":
                call.append(f"out List<string> {n}Core")
                post.append(f"{n} = ToTextItems({n}Core);")
            elif p["out"] and p["cs"] in IS_TYPES:
                sys.exit(f"Mss{a['name']}: {p['cs']} outputs are not supported by the adapter")
            elif p["out"]:
                call.append(f"out {n}")
            elif p["cs"] == "List<Condition>":
                call.append(f"ToConditions({n})")
            elif p["cs"] == "List<string>":
                call.append(f"ToStrings({n})")
            else:
                call.append(n)
        lines += ["", f"\t\tpublic void Mss{a['name']}({', '.join(sig)}) {{",
                  f"\t\t\t_core.Mss{a['name']}({', '.join(call)});"]
        lines += [f"\t\t\t{x}" for x in post]
        lines.append("\t\t}")
    lines += [
        "",
        "\t\tprivate static List<Condition> ToConditions(RLConditionRecordList list) {",
        "\t\t\tif (list == null) return new List<Condition>();",
        "\t\t\treturn new List<Condition>(list.ToArray(r => new Condition {",
        "\t\t\t\tPath = r.ssSTCondition.ssPath,",
        "\t\t\t\tOperator = r.ssSTCondition.ssOperator,",
        "\t\t\t\tValue = r.ssSTCondition.ssValue,",
        "\t\t\t\tCaseSensitive = r.ssSTCondition.ssCaseSensitive",
        "\t\t\t}));",
        "\t\t}",
        "",
        "\t\tprivate static List<string> ToStrings(RLTextItemRecordList list) {",
        "\t\t\tif (list == null) return new List<string>();",
        "\t\t\treturn new List<string>(list.ToArray(r => r.ssSTTextItem.ssValue));",
        "\t\t}",
        "",
        "\t\tprivate static RLTextItemRecordList ToTextItems(List<string> items) {",
        "\t\t\tvar list = new RLTextItemRecordList();",
        "\t\t\tif (items == null) return list;",
        "\t\t\tforeach (var s in items) {",
        "\t\t\t\tvar rec = new RCTextItemRecord(null);",
        "\t\t\t\tvar st = new STTextItemStructure(null);",
        "\t\t\t\tst.ssValue = s ?? \"\";",
        "\t\t\t\trec.ssSTTextItem = st;",
        "\t\t\t\tlist.Add(rec);",
        "\t\t\t}",
        "\t\t\treturn list;",
        "\t\t}",
        "\t}",
        "}",
        "",
    ]
    (SRC / "ListUtils.cs").write_bytes("\r\n".join(lines).encode("utf-8"))


def main() -> None:
    if not (BIN / "System.Text.Json.dll").exists():
        sys.exit(f"build ListUtils.O11 first: dotnet build {O11} -c Release")
    actions = parse_o11_interface()
    core = write_core_sources()
    write_adapter(actions)
    spec = build_spec(actions)
    (HERE / "spec.json").write_text(json.dumps(spec, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"spec.json: {len(spec['actions'])} actions, {len(spec['structures'])} structures, "
          f"{len(spec['sources'])} sources ({', '.join(core)} + ListUtils.cs), {len(spec['binaries'])} DLLs")


if __name__ == "__main__":
    main()
