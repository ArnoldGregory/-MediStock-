// ============================================================
//  MediStock.API — InvoiceParsingService
//  Reads supplier invoices (PDF or photo) into reviewable line items.
//    * Digital PDF  → PdfPig extracts the embedded text layer.
//    * Scanned PDF / image → render to bitmap → Tesseract OCR.
//  Output is deliberately "good enough to review": the Portal shows a
//  table where the user fixes names/quantities/prices before import.
// ============================================================

using SkiaSharp;
using Tesseract;
using UglyToad.PdfPig;

namespace MediStock.API.Services
{
    public class InvoiceLine
    {
        public int line_no { get; set; }
        public string description { get; set; } = "";
        public int quantity { get; set; }
        public decimal unit_cost { get; set; }
        public decimal? unit_sell_price { get; set; }
        public string? expiry_date { get; set; }
        public decimal? line_total { get; set; }
        public double confidence { get; set; }
        public bool skip { get; set; }
    }

    public class InvoiceParseResult
    {
        public string document_type { get; set; } = "ocr";
        public string? invoice_number { get; set; }
        public string? invoice_date { get; set; }
        public string? supplier_name { get; set; }
        public decimal? total { get; set; }
        public List<InvoiceLine> lines { get; set; } = new();
        public string note { get; set; } = "";
    }

    public static class InvoiceParsingService
    {
        private const int MaxLines = 200;

        private static readonly string[] SkipWords = new[]
        {
            "invoice", "inv no", "receipt", "delivery", "kephis", "date",
            "address", "tel", "phone", "email", "po box", "p.o.", "subt", "vat",
            "discount", "subtotal", "terms", "page", "cashier", "thank", "total",
            "tax", "balance", "credit", "kra", "struct", "liquor", "goods",
            "customer", "client", "seller", "buyer", "note", "attn", "amount due",
            "payable", "due", "www", ".com", ".co.", "m-pesa", "mpesa", "bank",
            "account", "swift", "currency", "item", "code", "description"
        };

        // ── Public entry ──────────────────────────────────────────────────────
        public static InvoiceParseResult Parse(byte[] fileBytes, string fileName)
        {
            var result = new InvoiceParseResult();
            if (fileBytes == null || fileBytes.Length == 0) { result.note = "Empty file."; return result; }

            string ext = (Path.GetExtension(fileName) ?? "").ToLowerInvariant();
            string text = "";
            bool isPdf = ext == ".pdf";

            if (isPdf)
            {
                text = TryPdfText(fileBytes);
                if (PdfTextUsable(text)) result.document_type = "digital";
                else { result.document_type = "ocr"; text = OcrPdf(fileBytes); }
            }
            else
            {
                result.document_type = "ocr";
                text = OcrImage(fileBytes);
            }

            if (string.IsNullOrWhiteSpace(text)) { result.note = "Could not read any text from the file."; return result; }

            ExtractHeader(result, text);

            foreach (var rawLine in text.Split('\n'))
            {
                if (result.lines.Count >= MaxLines) break;
                TryParseLine(rawLine, result, result.lines.Count + 1);
            }

            if (result.lines.Count == 0) result.note = "Read text but found no product lines. Try a clearer scan.";
            return result;
        }

        // ── Text sources ─────────────────────────────────────────────────────
        private static string TryPdfText(byte[] bytes)
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                using var pdf = PdfDocument.Open(bytes);
                foreach (var page in pdf.GetPages()) { sb.AppendLine(page.Text); }
                return sb.ToString();
            }
            catch { return ""; }
        }

        private static bool PdfTextUsable(string text)
        {
            int letters = text.Count(char.IsLetter);
            return letters > 300;
        }

        private static string OcrPdf(byte[] bytes)
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                using var stream = new MemoryStream(bytes);
                var options = new PDFtoImage.RenderOptions { Dpi = 200 };
                var pages = PDFtoImage.Conversion.ToImages(stream, false, null, options).ToList();
                foreach (var bmp in pages)
                {
                    using (bmp) sb.Append(OcrBitmap(bmp));
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            catch { return ""; }
        }

        private static string OcrImage(byte[] bytes)
        {
            try
            {
                using var bitmap = SKBitmap.Decode(bytes);
                if (bitmap == null) return "";
                return OcrBitmap(bitmap);
            }
            catch { return ""; }
        }

        private static string OcrBitmap(SKBitmap input)
        {
            try
            {
                byte[] png = EncodePng(input);
                string outDir = Path.GetTempPath();
                string file = Path.Combine(outDir, "medistock_invoice_" + Guid.NewGuid().ToString("N") + ".png");
                File.WriteAllBytes(file, png);
                try
                {
                    string tessdata = Path.Combine(AppContext.BaseDirectory, "tessdata");
                    if (!Directory.Exists(tessdata)) return "";
                    using var engine = new TesseractEngine(tessdata, "eng", EngineMode.Default);
                    engine.SetVariable("preserve_interword_spaces", "1");
                    using var pix = Pix.LoadFromFile(file);
                    using var page = engine.Process(pix);
                    return page.GetText();
                }
                finally { try { File.Delete(file); } catch { } }
            }
            catch { return ""; }
        }

        private static byte[] EncodePng(SKBitmap src)
        {
            // Upscale small images (phone photos) so OCR reads small digits reliably.
            float scale = 1f;
            if (src.Width < 1600) scale = 2f;
            if (src.Width < 900) scale = 3f;

            using var resized = new SKBitmap((int)(src.Width * scale), (int)(src.Height * scale));
            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
            src.ScalePixels(resized, sampling);
            using var img = SKImage.FromBitmap(resized);
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        // ── Header extraction ─────────────────────────────────────────────────
        private static void ExtractHeader(InvoiceParseResult r, string text)
        {
            r.invoice_number = FirstMatch(text, @"(?:INV(?:OICE)?|Invoice No|INV-|INV\.|No\.?)[ :#\-]*\s*([A-Z0-9][A-Z0-9/\-]{2,})");
            if (r.invoice_number != null && !System.Text.RegularExpressions.Regex.IsMatch(r.invoice_number, @"\d"))
                r.invoice_number = null; // OCR fragments like "oice" are not invoice numbers
            r.invoice_date = FirstMatch(text, @"Date[ :]*(\d{1,2}[/\-.]\d{1,2}[/\-.]\d{2,4})");
            r.invoice_date ??= FirstMatch(text, @"\b(\d{1,2}[/\-.]\d{1,2}[/\-.]\d{2,4})\b");
            r.supplier_name = FirstSupplierLine(text);
            r.total = FirstValue(text, @"(?:TOTAL|GRAND TOTAL|Amount Due|Balance Due)[ :]*K?E?S?\.?\s*([\d,]+\.\d{2})");
        }

        private static string? FirstSupplierLine(string text)
        {
            foreach (var line in text.Split('\n'))
            {
                var t = Clean(line);
                if (t.Length < 4 || t.Length > 60) continue;
                if (t.Contains("ltd", StringComparison.OrdinalIgnoreCase)
                    || t.Contains("pharma", StringComparison.OrdinalIgnoreCase)
                    || t.Contains("wholesale", StringComparison.OrdinalIgnoreCase)
                    || t.Contains("distributor", StringComparison.OrdinalIgnoreCase)
                    || t.Contains("supplier", StringComparison.OrdinalIgnoreCase)
                    || t.Contains("supplies", StringComparison.OrdinalIgnoreCase))
                    return t;
            }
            return null;
        }

        // ── Line parsing ──────────────────────────────────────────────────────
        private static void TryParseLine(string rawLine, InvoiceParseResult result, int lineNo)
        {
            var line = Clean(rawLine);
            if (line.Length < 3) return;
            if (StartsSkip(line)) return;

            // Grab the numeric tokens that look like money / quantities.
            var numberMatches = System.Text.RegularExpressions.Regex.Matches(line, @"\d[\d,]*\.\d{1,2}|\b\d+\b")
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(m => m.Value.Replace(",", ""))
                .ToList();

            if (numberMatches.Count == 0) return;

            // Description = everything before the last numeric token.
            var lastNumberIdx = LastNonNumberIndex(line);
            string description = lastNumberIdx > 0 ? line[..lastNumberIdx].Trim().TrimEnd(':', ';', '-', '+') : line;

            if (string.IsNullOrEmpty(description) || description.Length < 2) return;
            if (description.Count(char.IsLetter) < 2) return;

            var decimals = numberMatches
                .Where(n => n.Contains('.'))
                .Select(n => ParseDecimal(n))
                .Where(d => d > 0)
                .ToList();

            var ints = numberMatches
                .Where(n => !n.Contains('.'))
                .Select(n => ParseInt(n))
                .Where(i => i >= 0 && i < 1_000_000)
                .ToList();

            int qty = ints.Count > 0 ? ints[0] : 0;
            if (qty == 0 && description.EndsWith("x")) return; // "x 100" standalone qty lines

            if (qty == 0) return; // we need at least a quantity

            var item = new InvoiceLine { line_no = lineNo, description = description, quantity = qty };

            // Price heuristics:
            //   * cheapest decimal token  → unit cost
            //   * 3rd decimal token       → likely the owner's typed sell-price column
            //   * most expensive token    → line total when it differs from qty*unit
            if (decimals.Count > 0)
            {
                var sorted = decimals.OrderBy(d => d).ToList();
                item.unit_cost = sorted[0];
                if (decimals.Count >= 3) item.unit_sell_price = sorted.Count > 1 ? sorted[1] : null;

                decimal expected = qty * sorted[0];
                var maxD = sorted[^1];
                if (maxD != expected && maxD > expected + 0.004m) item.line_total = maxD;
                else if (maxD == expected) item.line_total = maxD;
            }

            item.confidence = (item.unit_cost > 0 ? 0.9 : 0.5);
            if (item.unit_sell_price.HasValue) item.confidence = Math.Min(0.9, item.confidence);

            result.lines.Add(item);
        }

        private static bool StartsSkip(string line)
        {
            var low = line.ToLowerInvariant();
            if (SkipWords.Any(w => low.Contains(w))) return true;
            if (low.StartsWith("c/o") || low.StartsWith("tel") || low.StartsWith("p.o")) return true;
            return false;
        }

        private static string Clean(string raw)
        {
            var t = System.Text.RegularExpressions.Regex.Replace(raw, @"[|_:]{2,}", " ");
            t = System.Text.RegularExpressions.Regex.Replace(t, @"\s+", " ").Trim();
            return t.Trim('|', '-', '.', ' ');
        }

        private static int LastNonNumberIndex(string line)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(line, @"\d[\d,]*\.\d{1,2}|\b\d{1,6}\b");
            if (matches.Count == 0) return 0;
            return matches[^1].Index;
        }

        private static string? FirstMatch(string text, string pattern)
        {
            var m = System.Text.RegularExpressions.Regex.Match(text, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }

        private static decimal? FirstValue(string text, string pattern)
        {
            var v = FirstMatch(text, pattern);
            return v == null ? null : ParseDecimal(v);
        }

        private static decimal ParseDecimal(string s)
        {
            decimal.TryParse(s.Replace("K", "").Replace("S", ""), System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal d);
            return d;
        }

        private static int ParseInt(string s)
        {
            int.TryParse(s, out int i);
            return i;
        }
    }
}