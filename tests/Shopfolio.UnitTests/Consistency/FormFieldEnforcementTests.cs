using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Validation;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The two-way equality of a field row and the code's validation (T547 criterion 6, KK6): the row names a rule, a limit or a requirement - the code must hold the check (row to code); the code holds a check - the row must
/// name it (code to row); and no kind of check or rule is skipped (the mapping tables hold every one, or the test fails). The checks that must fail read a changed copy of the sources or call the comparison with a
/// check removed, never the files (09 19.2 item 3).
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class FormFieldEnforcementTests
{
    private static readonly Lazy<string> Document = new(StateRoleDocument.Load);

    private static readonly Lazy<IReadOnlyDictionary<string, string>> Sources = new(() => NavigationSites.Load(RepositoryRoot.Find()));

    private static readonly IReadOnlySet<string> KnownRows = new HashSet<string>(StringComparer.Ordinal) { "7.2.7", "7.2.8.2", "7.2.8.3", "7.1.11.4" };

    private static IEnumerable<string> Run(string requirement, string[] marks, string[] lines, string[] ids, string[] limits)
        => FormFieldEnforcement.Problems("7.9.9.9", requirement, marks, lines, ids, limits, KnownRows, Sources.Value);

    // KK6: the attribute that carries a rule leaves the property - the row names the rule and the code no longer checks it.
    [Fact]
    public void Attribute_RemovedFromTheProperty_IsReported()
    {
        Assert.Contains(Run("Zorunlu", [], [], ["7.3.2.1"], []), p => p.StartsWith("RuleWithoutEnforcement:", StringComparison.Ordinal) && p.Contains("7.3.2.1", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", ["InvalidEmailAttribute"], [], ["7.3.2.1"], []), p => p.StartsWith("RequirementWithoutEnforcement:", StringComparison.Ordinal));
        Assert.Empty(Run("Zorunlu", ["FieldRequiredAttribute", "InvalidEmailAttribute"], [], ["7.3.2.1"], []));
    }

    // KK6: the requirement leaves the code - the row says "Zorunlu" and nothing makes the field required.
    [Fact]
    public void Requirement_RemovedFromTheCode_IsReported()
    {
        Assert.Contains(Run("Zorunlu", ["InvalidIbanAttribute"], [], ["7.3.2.7"], []), p => p.StartsWith("RequirementWithoutEnforcement:", StringComparison.Ordinal));
        Assert.Contains(Run("Yayın için zorunlu", [], [], [], []), p => p.StartsWith("RequirementWithoutEnforcement:", StringComparison.Ordinal));
        Assert.DoesNotContain(Run("İsteğe bağlı", ["InvalidIbanAttribute"], [], ["7.3.2.7"], []), p => p.StartsWith("Requirement", StringComparison.Ordinal));
    }

    // KK6: the code requires what the row calls optional.
    [Fact]
    public void Requirement_AddedToTheCode_IsReported()
    {
        Assert.Contains(Run("İsteğe bağlı", ["FieldRequiredAttribute"], [], [], []), p => p.StartsWith("RequirementContradictsCode:", StringComparison.Ordinal));
        Assert.Contains(Run("Seçim", [], ["Required|Pages/Product.cshtml.cs#nameof(AddCartItemInput.VariantId), FieldCodes.FieldRequired"], [], []), p => p.StartsWith("RequirementContradictsCode:", StringComparison.Ordinal));
    }

    // KK6: the code checks a kind the row does not name; a rule the row names is another kind than the code's.
    [Fact]
    public void Check_TheRowDoesNotName_IsReported()
    {
        Assert.Contains(Run("Zorunlu", ["FieldRequiredAttribute", "InvalidIbanAttribute"], [], [], []), p => p.StartsWith("RuleMissesAttribute:", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", ["FieldRequiredAttribute", "InvalidIbanAttribute"], [], ["7.3.2.8"], []), p => p.StartsWith("RuleWithoutEnforcement:", StringComparison.Ordinal) && p.Contains("7.3.2.8", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", ["FieldRequiredAttribute"], ["P-27|Modules/Catalog/ProductSaves.cs#nameof(ProductSaveCommand.Name), ErrorCode.TextTooLong, ProductConstants.ProductNameMaxLength"], [], []), p => p.StartsWith("RuleMissesAttribute:", StringComparison.Ordinal));
    }

    // KK6: a parameter or a limit the row names is read by a line of the field's check; without the line the row is reported.
    [Fact]
    public void Parameter_AndLimit_WithoutALine_AreReported()
    {
        Assert.Contains(Run("İsteğe bağlı", [], [], ["P-9"], []), p => p.StartsWith("RuleWithoutEnforcement:", StringComparison.Ordinal) && p.Contains("P-9", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], [], [], ["L-4"]), p => p.StartsWith("RuleWithoutEnforcement:", StringComparison.Ordinal) && p.Contains("L-4", StringComparison.Ordinal));
        Assert.DoesNotContain(Run("İsteğe bağlı", [], ["L-4|Modules/CorporateContent/ContactFormSending.cs#_attempts.RecordAsync(AttemptLimit.L4, byIp, ct)"], [], ["L-4"]), p => p.StartsWith("Rule", StringComparison.Ordinal));
    }

    // KK6: a kind of check or a rule that the tables do not hold is never skipped.
    [Fact]
    public void KindOrRule_NotInTheTables_IsReported()
    {
        Assert.Contains(Run("İsteğe bağlı", ["InvalidSomethingAttribute"], [], [], []), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], ["Unknown|Pages/Product.cshtml.cs#FieldCodes.FieldRequired"], [], []), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], [], ["7.3.2.99"], []), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], [], ["P-99"], []), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], [], [], ["L-99"]), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], [], ["7.9.1.1"], []), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
    }

    // KK6: an enforcement line whose token is not of its kind, or that is not in the file, is reported.
    [Fact]
    public void EnforcementLine_OfAnotherKindOrNotInTheFile_IsReported()
    {
        Assert.Contains(Run("Zorunlu", [], ["Email|Pages/Product.cshtml.cs#nameof(AddCartItemInput.VariantId), FieldCodes.FieldRequired"], [], []), p => p.StartsWith("EnforcementNotInCode:", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", [], ["Required|Pages/Product.cshtml.cs#nameof(AddCartItemInput.NoSuchField), FieldCodes.FieldRequired"], [], []), p => p.StartsWith("EnforcementNotInCode:", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", [], ["Required|Pages/NoSuchFile.cs#FieldCodes.FieldRequired"], [], []), p => p.StartsWith("EnforcementNotInCode:", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", [], ["Required without a separator"], [], []), p => p.StartsWith("EnforcementNotInCode:", StringComparison.Ordinal));
    }

    // KK6 on the real table: the piece of code of every enforcement line is held exactly once by its file, and removing that piece from the sources reports exactly the rows that hold that code - the rows that
    // use the piece and the rows whose own piece overlaps it - never the row of another place (the evidence of a row is its own code, not another field's or another flow's).
    [Fact]
    public void EveryEnforcementLine_RemovedFromTheCode_ReportsExactlyTheRowsThatHoldThatCode()
    {
        List<string> wrong = [];
        List<(string Row, string File, string Piece)> all =
        [
            .. FormFieldEnforcementTable.Lines.SelectMany(r => r.Value.Select(l => (r.Key, File: FileOf(l), Piece: PieceOf(l)))),
        ];
        foreach ((string file, string piece) in all.Select(a => (a.File, a.Piece)).Distinct())
        {
            string text = FormFieldCheck.Collapse(Sources.Value[file]);
            string collapsed = FormFieldCheck.Collapse(piece);
            int at = text.IndexOf(collapsed, StringComparison.Ordinal);
            if (at < 0 || FormFieldCheck.Count(text, collapsed) != 1)
            {
                wrong.Add($"{file}: '{piece}' is held {FormFieldCheck.Count(text, collapsed)} times");
                continue;
            }

            string removed = text[..at] + "RemovedFromTheCode" + text[(at + collapsed.Length)..];
            IReadOnlyList<string> problems = FormFieldCheck.Problems(Document.Value, new Dictionary<string, string>(Sources.Value, StringComparer.Ordinal) { [file] = removed });
            string[] reported = [.. problems.Where(p => p.StartsWith("EnforcementNotInCode:", StringComparison.Ordinal) || p.StartsWith("EnforcementAmbiguous:", StringComparison.Ordinal)).Select(RowOf).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
            string[] expected =
            [
                .. all.Where(a => a.File == file && Overlaps(text, collapsed, at, FormFieldCheck.Collapse(a.Piece))).Select(a => a.Row).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal),
            ];
            if (!reported.SequenceEqual(expected))
            {
                wrong.Add($"{file}: removing '{piece}' reported [{string.Join(", ", reported)}], the rows that hold the code are [{string.Join(", ", expected)}]");
            }
        }

        Assert.True(wrong.Count == 0, "A removed piece of code was not reported by exactly its own rows:\n" + string.Join('\n', wrong));
    }

    // KK6 on the real code, the changes that remove or change a rule: the limit of the e-mail change moves to another limit, the quantity check of one handler leaves, a field leaves the required list - the
    // row of that flow is reported and the row of the flow that shares the helper or the text is not reported for the other's change.
    [Fact]
    public void ChangeOfALimit_OfAFlowsQuantityCheck_AndOfARequiredList_ReportsTheRowsOfThatFlow()
    {
        IReadOnlyList<string> limit = ProblemsWith("Infrastructure/Security/EmailChangeRules.cs", s => s.Replace("attempts.RecordAsync(AttemptLimit.L9, byEmail, ct)", "attempts.RecordAsync(AttemptLimit.L3, byEmail, ct)", StringComparison.Ordinal));
        foreach (string row in new[] { "7.1.15.2", "7.1.15.4", "7.2.24.2", "7.2.24.4" })
        {
            Assert.Contains(limit, p => p.StartsWith($"EnforcementNotInCode:{row}:", StringComparison.Ordinal));
        }

        IReadOnlyList<string> quantity = ProblemsWith("Pages/Panel/OrderDetail.ItemSteps.cs", s => Cut(s, "Task<IActionResult> ArriveAsync(", "if (!TryReadQuantity(input.Quantity, out int read))", "if (false)"));
        Assert.Contains(quantity, p => p.StartsWith("EnforcementNotInCode:7.2.12.2:", StringComparison.Ordinal));
        Assert.DoesNotContain(quantity, p => p.StartsWith("EnforcementNotInCode:7.2.11.2:", StringComparison.Ordinal));

        IReadOnlyList<string> required = ProblemsWith("Modules/Settings/CompanyIdentityRules.cs", s => Cut(s, "CommonFields =", "nameof(CompanyIdentityInput.Phone),", string.Empty));
        Assert.Contains(required, p => p.StartsWith("EnforcementNotInCode:7.2.19.4:", StringComparison.Ordinal));
        Assert.DoesNotContain(required, p => p.StartsWith("EnforcementNotInCode:7.2.19.3:", StringComparison.Ordinal));
        IReadOnlyList<string> province = ProblemsWith("Modules/Settings/CompanyIdentityRules.cs", s => Cut(s, "CommonFields =", "nameof(CompanyIdentityInput.ProvinceId),", string.Empty));
        Assert.Contains(province, p => p.StartsWith("EnforcementNotInCode:7.2.19.3:", StringComparison.Ordinal));
    }

    // A piece of code held twice by its file is not the evidence of a place: the row is reported as ambiguous (the same line copied elsewhere in the file).
    [Fact]
    public void EnforcementLine_HeldTwiceInItsFile_IsReported()
    {
        string line = FormFieldEnforcementTable.Lines["7.2.11.2"][0];
        string file = FileOf(line);
        IReadOnlyList<string> problems = ProblemsWith(file, s => s + "\n" + PieceOf(line));

        Assert.Contains(problems, p => p.StartsWith("EnforcementAmbiguous:7.2.11.2:", StringComparison.Ordinal));
    }

    // A row whose every line another form's row also has has no evidence of its own: reported; one line of its own is enough.
    [Fact]
    public void Row_WithNoLineOfItsOwn_IsReported()
    {
        Dictionary<string, string[]> shared = new(StringComparer.Ordinal)
        {
            ["7.1.1.1"] = ["L-1|Infrastructure/Security/PasswordRegime.cs#AttemptLimit.L1, subject"],
            ["7.2.1.1"] = ["L-1|Infrastructure/Security/PasswordRegime.cs#AttemptLimit.L1, subject"],
            ["7.2.1.2"] = ["L-1|Infrastructure/Security/PasswordRegime.cs#AttemptLimit.L1, subject"],
        };
        string[] rows = ["7.1.1.1", "7.2.1.1", "7.2.1.2"];

        Assert.Contains(FormFieldCheck.OwnLines(shared, rows), p => p.StartsWith("EnforcementNotDistinctive:7.1.1.1:", StringComparison.Ordinal));
        Assert.Contains(FormFieldCheck.OwnLines(shared, rows), p => p.StartsWith("EnforcementNotDistinctive:7.2.1.1:", StringComparison.Ordinal));
        Assert.Contains(FormFieldCheck.OwnLines(shared, rows), p => p.StartsWith("EnforcementNotDistinctive:7.2.1.2:", StringComparison.Ordinal));

        shared["7.2.1.1"] = [.. shared["7.2.1.1"], "L-1|Modules/Administration/AdministrationModule.cs#_passwords.CheckPasswordAsync(_accounts, email, password, ipAddress, recognizedBrowserMark, ct)"];
        string[] after = [.. FormFieldCheck.OwnLines(shared, rows)];
        Assert.DoesNotContain(after, p => p.StartsWith("EnforcementNotDistinctive:7.2.1.1:", StringComparison.Ordinal));
        Assert.Contains(after, p => p.StartsWith("EnforcementNotDistinctive:7.1.1.1:", StringComparison.Ordinal));
    }

    // The real table: every row has a line of its own (no row stands on code that only another form's row also names).
    [Fact]
    public void EveryRowOfTheTable_HasALineOfItsOwn()
    {
        string[] none = [.. FormFieldCheck.OwnLines(FormFieldEnforcementTable.Lines, FormFieldEnforcementTable.Lines.Keys)];

        Assert.True(none.Length == 0, "Rows without a line of their own:\n" + string.Join('\n', none));
    }

    // The 8.2.3 id is the separator's format and names no check; the net quantity's scale is K-1013 and the net quantity check answers to it (04 7.2.4.13).
    [Fact]
    public void NetQuantityScale_IsTheIdOfK1013_AndTheSeparatorFormatNamesNoCheck()
    {
        Assert.Empty(FormFieldEnforcement.RuleKinds["8.2.3"]);
        Assert.Equal(["NetQuantity"], FormFieldEnforcement.RuleKinds["K-1013"]);
        Assert.DoesNotContain(Run("İsteğe bağlı", ["InvalidNetQuantityAttribute"], [], ["8.2.3", "K-1013"], []), p => p.StartsWith("Rule", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", ["InvalidNetQuantityAttribute"], [], ["8.2.3"], []), p => p.StartsWith("RuleMissesAttribute:", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], [], ["K-1013"], []), p => p.StartsWith("RuleWithoutEnforcement:", StringComparison.Ordinal));
    }

    private static string FileOf(string line) => line[(line.IndexOf('|', StringComparison.Ordinal) + 1)..line.IndexOf('#', StringComparison.Ordinal)];

    private static string PieceOf(string line) => line[(line.IndexOf('#', StringComparison.Ordinal) + 1)..];

    private static string RowOf(string problem) => problem[(problem.IndexOf(':', StringComparison.Ordinal) + 1)..problem.IndexOf(':', problem.IndexOf(':', StringComparison.Ordinal) + 1)];

    private static bool Overlaps(string text, string removed, int at, string other)
    {
        int start = text.IndexOf(other, StringComparison.Ordinal);
        return start >= 0 && start < at + removed.Length && at < start + other.Length;
    }

    private static IReadOnlyList<string> ProblemsWith(string file, Func<string, string> change)
        => FormFieldCheck.Problems(Document.Value, new Dictionary<string, string>(Sources.Value, StringComparer.Ordinal) { [file] = change(Sources.Value[file]) });

    /// <summary>The text with the first <paramref name="what"/> after <paramref name="after"/> replaced; the change is an error of the test when the text is not there.</summary>
    private static string Cut(string text, string after, string what, string with)
    {
        int from = text.IndexOf(after, StringComparison.Ordinal);
        int at = from < 0 ? -1 : text.IndexOf(what, from, StringComparison.Ordinal);
        Assert.True(at >= 0, $"'{what}' is not in the text after '{after}'.");
        return text[..at] + with + text[(at + what.Length)..];
    }

    // KK6 on the real code: every rule attribute of every property place of the inventory, taken off the property (the marks given to the comparison without it), reports the row unless the same kind is
    // still checked by another attribute or line of the row.
    [Fact]
    public void EveryRuleAttribute_TakenOffTheProperty_IsNoLongerEvidence()
    {
        List<string> silent = [];
        foreach (FieldEntry entry in FormFieldInventory.Entries)
        {
            string[] marks = [.. entry.Where.Split(" + ", StringSplitOptions.RemoveEmptyEntries).Where(w => !w.Contains('#', StringComparison.Ordinal)).SelectMany(w => RuleMarksOf(w))];
            foreach (string mark in marks.Distinct(StringComparer.Ordinal))
            {
                string[] without = [.. marks.Where(m => m != mark)];
                string kind = FormFieldEnforcement.AttributeKinds[mark];
                string[] lines = FormFieldEnforcementTable.Lines.TryGetValue(entry.Row, out string[]? held) ? held : [];
                bool stillChecked = without.Any(m => FormFieldEnforcement.AttributeKinds[m] == kind) || lines.Any(l => l.StartsWith(kind + "|", StringComparison.Ordinal));
                if (stillChecked)
                {
                    continue;
                }

                // The row's own claim: the field was required by this attribute, or a rule of this kind was named.
                bool claimed = kind == "Required" || FormFieldEnforcement.RuleKinds.Values.Any(v => v.Contains(kind, StringComparer.Ordinal));
                bool reported = FormFieldEnforcement.Problems(entry.Row, kind == "Required" ? "Zorunlu" : "İsteğe bağlı", without, lines, kind == "Required" ? [] : RuleIdsOf(kind), [], KnownRows, Sources.Value)
                    .Any(p => p.StartsWith("Rule", StringComparison.Ordinal) || p.StartsWith("Requirement", StringComparison.Ordinal));
                if (!claimed || !reported)
                {
                    silent.Add($"{entry.Row}: {mark}");
                }
            }
        }

        Assert.True(silent.Count == 0, "An attribute taken off a property was not reported:\n" + string.Join('\n', silent));
    }

    // No rule attribute of the code is outside the mapping table: a new one fails here until it is mapped.
    [Fact]
    public void EveryRuleAttributeOfTheCode_IsInTheKindTable()
    {
        string[] missing =
        [
            .. typeof(Program).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(FieldRuleAttribute).IsAssignableFrom(t))
                .Select(t => t.Name)
                .Where(n => !FormFieldEnforcement.AttributeKinds.ContainsKey(n)),
        ];

        Assert.True(missing.Length == 0, "Rule attributes without a kind: " + string.Join(", ", missing));
    }

    // No field type of 04 7.3.2 is outside the rule table: a new row of the table fails here until it is mapped.
    [Fact]
    public void EveryFieldType_OfTheDocument_IsInTheRuleTable()
    {
        string[] types = [.. FieldTypeRow().Matches(Document.Value).Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal)];
        string[] missing = [.. types.Where(t => !FormFieldEnforcement.RuleKinds.ContainsKey(t))];

        Assert.NotEmpty(types);
        Assert.True(missing.Length == 0, "Field types without a rule kind: " + string.Join(", ", missing));
    }

    // Every kind the tables hold has tokens, and every id the rule table names a kind for names kinds that the kind table holds.
    [Fact]
    public void RuleTable_NamesOnlyKindsOfTheKindTable()
    {
        string[] unknown =
        [
            .. FormFieldEnforcement.RuleKinds.Values.SelectMany(v => v).Concat(FormFieldEnforcement.AttributeKinds.Values)
                .Where(k => !FormFieldEnforcement.KindTokens.ContainsKey(k) && k is not "Color" && k is not "CheckboxAccepted")
                .Distinct(StringComparer.Ordinal),
        ];

        Assert.True(unknown.Length == 0, "Kinds without tokens: " + string.Join(", ", unknown));
    }

    // D-1656 (code to row): a check that names a field and its error code in the code is held by a line of the tables - the row's check is not only the table's word.
    [Fact]
    public void EveryCodeCheck_OfAField_IsHeldByALineOfTheTable()
    {
        IReadOnlyList<string> problems = FormFieldCodeCalls.Problems(Sources.Value, FormFieldEnforcementTable.Lines, FormFieldCodeInventory.Unlisted);

        Assert.True(problems.Count == 0, string.Join('\n', problems));
    }

    private static IReadOnlyList<string> CodeCheckProblems(string file, Func<string, string> change, IReadOnlyDictionary<string, string[]>? lines = null, IReadOnlyList<FormFieldCodeCall>? unlisted = null)
    {
        Dictionary<string, string> copy = new(Sources.Value, StringComparer.Ordinal) { [file] = change(Sources.Value[file]) };
        Assert.NotEqual(Sources.Value[file], copy[file]);
        return FormFieldCodeCalls.Problems(copy, lines ?? FormFieldEnforcementTable.Lines, unlisted ?? FormFieldCodeInventory.Unlisted);
    }

    // D-1656: a check added to a module, a handler or a page - a field and its error code in one expression - that no line of the table holds is reported; the two writings of a field (nameof, a literal) are both seen.
    [Fact]
    public void CodeCheck_AddedWithoutALineOfTheTable_IsReported()
    {
        IReadOnlyList<string> byName = CodeCheckProblems("Modules/Settings/FirmSettings.cs", s => s + "\nrejections.Add(new FieldRejection(nameof(HomepageInput.MenuNameProducts), FieldCodes.InvalidUrl));");
        IReadOnlyList<string> byLiteral = CodeCheckProblems("Modules/Catalog/ProductSaves.cs", s => s + "\nerrors.Add(new ProductSaveFieldError(\"Options[0].Colour\", ErrorCode.FieldRequired));");
        IReadOnlyList<string> byConstant = CodeCheckProblems("Modules/CorporateContent/ContactFormSending.cs", s => s + "\nfields.Add(ContactFormFields.Message, ErrorCode.InvalidEmail);");
        IReadOnlyList<string> codeFirst = CodeCheckProblems("Pages/Panel/Coupons.cshtml.cs", s => s + "\nyield return FieldError.Result(FieldCodes.InvalidPhone, nameof(Percentage));");

        Assert.Contains(byName, p => p.StartsWith("CodeCheckNotInTable: Modules/Settings/FirmSettings.cs: MenuNameProducts is checked with InvalidUrl", StringComparison.Ordinal));
        Assert.Contains(byLiteral, p => p.StartsWith("CodeCheckNotInTable: Modules/Catalog/ProductSaves.cs: Options[0].Colour is checked with FieldRequired", StringComparison.Ordinal));
        Assert.Contains(byConstant, p => p.StartsWith("CodeCheckNotInTable: Modules/CorporateContent/ContactFormSending.cs: Message is checked with InvalidEmail", StringComparison.Ordinal));
        Assert.Contains(codeFirst, p => p.StartsWith("CodeCheckNotInTable: Pages/Panel/Coupons.cshtml.cs: Percentage is checked with InvalidPhone", StringComparison.Ordinal));
    }

    // D-1656: a check of the code changes its code (the end date of a coupon is now checked as a percentage) - the new pair is no line of the table; and a check of a field that a line holds, moved to the
    // field of another row, is reported the same way.
    [Fact]
    public void CodeCheck_ChangedToAnotherCode_OrAnotherField_IsReported()
    {
        IReadOnlyList<string> code = CodeCheckProblems("Modules/Catalog/CouponRules.cs", s => s.Replace("nameof(CouponDefinition.EndDate), ErrorCode.InvalidDateRange", "nameof(CouponDefinition.EndDate), ErrorCode.InvalidPercentage", StringComparison.Ordinal));
        IReadOnlyList<string> field = CodeCheckProblems("Modules/Catalog/CouponRules.cs", s => s.Replace("nameof(CouponDefinition.Code), ErrorCode.FieldRequired", "nameof(CouponDefinition.Kind), ErrorCode.FieldRequired", StringComparison.Ordinal));

        Assert.Contains(code, p => p.StartsWith("CodeCheckNotInTable: Modules/Catalog/CouponRules.cs: EndDate is checked with InvalidPercentage", StringComparison.Ordinal));
        Assert.Contains(field, p => p.StartsWith("CodeCheckNotInTable: Modules/Catalog/CouponRules.cs: Kind is checked with FieldRequired", StringComparison.Ordinal));
    }

    // D-1656: the table's line leaves (the row's check is no longer held by a line) - the check of the code is reported; the tables and the code are compared in both directions, so a line of the table
    // that is not in the code is EnforcementNotInCode (the line held exactly once by its file).
    [Fact]
    public void LineOfTheTable_Removed_ReportsTheCheckOfTheCode()
    {
        Dictionary<string, string[]> lines = FormFieldEnforcementTable.Lines.ToDictionary(l => l.Key, l => l.Value.Where(x => !x.Contains("ErrorCode.InvalidDateRange)); }", StringComparison.Ordinal)).ToArray(), StringComparer.Ordinal);

        IReadOnlyList<string> problems = FormFieldCodeCalls.Problems(Sources.Value, lines, FormFieldCodeInventory.Unlisted);

        Assert.Contains(problems, p => p.StartsWith("CodeCheckNotInTable: Modules/Catalog/CouponRules.cs: EndDate is checked with InvalidDateRange", StringComparison.Ordinal));
    }

    [Fact]
    public void CodeCheckExemption_ThatIsNotInTheCode_OrIsHeldByALine_IsReported()
    {
        FormFieldCodeCall gone = new("Modules/Catalog/CouponRules.cs", "Kind", "InvalidEmail");
        FormFieldCodeCall held = new("Modules/Catalog/CouponRules.cs", "EndDate", "InvalidDateRange");

        IReadOnlyList<string> problems = FormFieldCodeCalls.Problems(Sources.Value, FormFieldEnforcementTable.Lines, [.. FormFieldCodeInventory.Unlisted, gone, held]);

        Assert.Contains(problems, p => p.StartsWith("DeadExemption: Modules/Catalog/CouponRules.cs: Kind, InvalidEmail", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.StartsWith("ExemptAndHeld: Modules/Catalog/CouponRules.cs: EndDate, InvalidDateRange", StringComparison.Ordinal));
    }

    [Fact]
    public void CodeCheckExemptions_HaveAReason_AndTheScanSeesTheCodesOfTheKinds()
    {
        Assert.All(FormFieldCodeInventory.Exemptions, e => Assert.False(string.IsNullOrWhiteSpace(e.Reason)));
        Assert.Equal(FormFieldCodeInventory.Exemptions.Count, FormFieldCodeInventory.Unlisted.Distinct().Count());
        string[] codes = ["FieldRequired", "InvalidEmail", "InvalidPhone", "InvalidProvince", "InvalidListValue", "InvalidIban", "InvalidAmount", "InvalidPercentage", "InvalidQuantity", "InvalidDate", "InvalidDateRange", "InvalidUrl", "TextTooLong", "InvalidImage", "InvalidColor", "InvalidNetQuantity", "CheckboxNotAccepted", "PasswordTooWeak", "FilledFieldCleared"];
        Assert.All(codes, c => Assert.Contains(c, FormFieldCodeCalls.Codes));
    }

    private static IEnumerable<string> RuleMarksOf(string where)
    {
        string[] parts = where.Split('.');
        if (parts.Length < 3)
        {
            return [];
        }

        string space = ".Pages." + parts[^3];
        Type? type = typeof(Program).Assembly.GetTypes().SingleOrDefault(t => t.Name == parts[^2] && t.Namespace is not null && t.Namespace.EndsWith(space, StringComparison.Ordinal));
        PropertyInfo? property = type?.GetProperty(parts[^1], BindingFlags.Instance | BindingFlags.Public);
        return property is null ? [] : FormFieldEnforcement.RuleMarks(property);
    }

    private static string[] RuleIdsOf(string kind) => [.. FormFieldEnforcement.RuleKinds.Where(r => r.Value.Contains(kind, StringComparer.Ordinal)).Select(r => r.Key).Take(1)];

    [GeneratedRegex(@"^\| (7\.3\.2\.\d+) \|", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FieldTypeRow();
}
