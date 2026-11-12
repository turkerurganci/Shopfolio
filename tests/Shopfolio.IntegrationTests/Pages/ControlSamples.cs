using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Pages.Components;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>
/// A storefront page that draws the control components of T544 with the real shared parts (04 OB-16 to OB-19): the coupon field inside an order summary, the confirmation boxes above the confirm button, the
/// gallery and single file uploads of a panel form and the upload progress. The page decides nothing the components decide: the boxes, the refusals and the texts are the components'.
/// </summary>
public sealed class ControlSampleModel : PageModel
{
    public const string Path = "/sss/kontrol";

    public static readonly ConsentBasket Basket = new(["Yazı tipi paketi"], []);

    public static readonly IReadOnlyList<string> BoxOrder = ConsentBoxes.FormOrder("Input.", Basket);

    public ControlSampleInput Input { get; set; } = new();

    public bool Confirmed { get; private set; }

    public string? CouponMode { get; private set; }

    public bool Uploading { get; private set; }

    public IActionResult OnGet(string? coupon, bool uploading = false)
    {
        CouponMode = coupon;
        Uploading = uploading;
        return Page();
    }

    // The confirmation: a form sent with a required box unticked does not make the order (04 2.12.7.3).
    public IActionResult OnPostConfirm(ControlSampleInput input)
    {
        Input = input;
        ConsentSent sent = new(ConsentSent.IsTicked(input.AcceptPreInformation), ConsentSent.IsTicked(input.AcceptDistanceSales), ConsentSent.IsTicked(input.AcceptDigital), ConsentSent.IsTicked(input.AcceptService));
        IReadOnlyList<ConsentKind> missing = ConsentBoxes.Missing(Basket, sent);
        if (missing.Count > 0)
        {
            ConsentBoxes.AddRefusals(ModelState, "Input.", missing);
            return FormResults.Redisplay(this);
        }

        Confirmed = true;
        return Page();
    }

    // The upload: a file the component refuses is not kept; the form is shown again with the reason at the field (04 2.12.8.3). The page decides nothing: the refusal is UploadChecks'.
    public async Task<IActionResult> OnPostUploadAsync(
        [FromForm(Name = "Input.Gallery.Files")] List<IFormFile>? galleryFiles,
        [FromForm(Name = "Input.Logo")] IFormFile? logo,
        bool goneUpload = false,
        int existingImages = 0)
    {
        UploadRefusal? count = UploadChecks.CheckCount(UploadSurface.ProductGallery, existingImages, galleryFiles?.Count ?? 0);
        if (count is not null)
        {
            UploadChecks.AddRefusal(ModelState, "Input.Gallery.Files", count);
        }

        foreach (IFormFile file in galleryFiles ?? [])
        {
            UploadRefusal? refusal = await CheckAsync(UploadSurface.ProductGallery, file, existingImages);
            if (refusal is not null)
            {
                UploadChecks.AddRefusal(ModelState, "Input.Gallery.Files", refusal);
                break;
            }
        }

        if (logo is not null && await CheckAsync(UploadSurface.Logo, logo, 0) is { } logoRefusal)
        {
            UploadChecks.AddRefusal(ModelState, "Input.Logo", logoRefusal);
        }

        if (goneUpload)
        {
            UploadChecks.AddMissingUpload(ModelState, "Input.File");
        }

        return ModelState.IsValid ? Page() : FormResults.Redisplay(this);
    }

    private static async Task<UploadRefusal?> CheckAsync(UploadSurface surface, IFormFile file, int existingCount)
    {
        byte[] head = new byte[16];
        await using Stream stream = file.OpenReadStream();
        int read = await stream.ReadAsync(head);
        return UploadChecks.CheckImage(surface, head.AsSpan(0, read), file.Length, existingCount);
    }

    public ConsentSent Sent() => new(
        ConsentSent.IsTicked(Input.AcceptPreInformation),
        ConsentSent.IsTicked(Input.AcceptDistanceSales),
        ConsentSent.IsTicked(Input.AcceptDigital),
        ConsentSent.IsTicked(Input.AcceptService));

    /// <summary>The coupon field the query names: the closed row by default.</summary>
    public CouponFieldView Coupon() => CouponMode switch
    {
        "open" => CouponFields.Open("Input.Coupon"),
        "rejected" => CouponFields.Open("Input.Coupon", "yanlis", CouponOutcome.Rejected),
        "below" => CouponFields.Open("Input.Coupon", "kucuk", CouponOutcome.BelowMinimumBasket, 500m),
        "applied" => CouponFields.Applied("YAZ10", 25m, "Input.Coupon"),
        "limit" => CouponFields.LimitClosed(),
        _ => CouponFields.Closed("Input.Coupon"),
    };

    public static IReadOnlyList<GalleryImage> Images { get; } =
    [
        new GalleryImage(new Guid("00000000-0000-0000-0000-0000000000b1"), "/img/b1.webp", "Ön yüz", IsMain: false, Order: 2),
        new GalleryImage(new Guid("00000000-0000-0000-0000-0000000000b2"), "/img/b2.webp", null, IsMain: true, Order: 1),
        new GalleryImage(new Guid("00000000-0000-0000-0000-0000000000b3"), "/img/b3.webp", "Arka yüz", IsMain: false, Order: 3),
    ];
}

/// <summary>The form of the control sample.</summary>
public sealed class ControlSampleInput : IFormInput
{
    public string? Coupon { get; set; }

    public string? AcceptPreInformation { get; set; }

    public string? AcceptDistanceSales { get; set; }

    public string? AcceptDigital { get; set; }

    public string? AcceptService { get; set; }
}
