using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The Turkish names the operation trail screen shows (T522; 04 9.23.4; 02 10.3.1): the eight areas and the operations of <see cref="AuditOperation"/>. A stored code the application does not recognize
/// (an operation of an older or newer version) is shown as the code itself, so a row is never hidden or mislabeled (<see cref="AuditOperationText.Display"/>).
/// </summary>
public static class AuditTexts
{
    public static string AreaOf(AuditArea area) => area switch
    {
        AuditArea.CompanyAndBranding => "Firma kimliği ve marka",
        AuditArea.Publication => "Yayın durumu",
        AuditArea.Order => "Sipariş",
        AuditArea.AdminAccounts => "Yönetici hesapları",
        AuditArea.LegalTextVersion => "Yasal metin sürümü",
        AuditArea.Settings => "Ayarlar",
        AuditArea.Export => "Dışa aktarma",
        AuditArea.CustomerDeletion => "Müşteri hesabı silme",
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, "The trail has eight areas (02 10.3.1)."),
    };

    /// <summary>The Turkish name of an operation, or the stored code itself when the code is not one of <see cref="AuditOperation"/>.</summary>
    public static string OperationOf(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        return AuditOperationText.TryParse(code, out AuditOperation operation) ? OperationOf(operation) : AuditOperationText.Display(code);
    }

    public static string OperationOf(AuditOperation operation) => operation switch
    {
        AuditOperation.UpdateCompany => "Firma kimliğini güncelledi",
        AuditOperation.UpdateBranding => "Markayı güncelledi",
        AuditOperation.ChangePublishStatus => "Yayın durumunu değiştirdi",
        AuditOperation.MarkBankTransferPaid => "Havaleyi ödendi işaretledi",
        AuditOperation.MarkShipped => "Kargoya verdi",
        AuditOperation.MarkDelivered => "Teslim edildi işaretledi",
        AuditOperation.SetDeliveryOrderAgainstWithdrawal => "Teslim sırasını cayma beyanına göre belirledi",
        AuditOperation.MarkDeliveryFailed => "Teslimatı başarısız işaretledi",
        AuditOperation.CompleteService => "Hizmeti tamamladı",
        AuditOperation.CancelByCompany => "Siparişi firma olarak iptal etti",
        AuditOperation.RefundManually => "İadeyi elle işledi",
        AuditOperation.ReceiveReturn => "İade malını teslim aldı",
        AuditOperation.ReceiveReturnWithIbanRequest => "İade malını teslim aldı ve IBAN istedi",
        AuditOperation.OpenRefundIbanRequest => "İade IBAN isteğini açtı",
        AuditOperation.RejectReturn => "İadeyi reddetti",
        AuditOperation.CorrectAddress => "Adresi düzeltti",
        AuditOperation.ReduceItemQuantity => "Kalem adedini azalttı",
        AuditOperation.RemoveItem => "Kalemi çıkardı",
        AuditOperation.CorrectShippingInfo => "Kargo bilgisini düzeltti",
        AuditOperation.CorrectDeliveryDate => "Teslim tarihini düzeltti",
        AuditOperation.CorrectReturnArrivalDate => "İade ulaşma tarihini düzeltti",
        AuditOperation.CorrectTransition => "Geçişi düzeltti",
        AuditOperation.SaveInternalNote => "İç not yazdı",
        AuditOperation.CloseNotReturned => "Malı dönmeyen kalemi kapattı",
        AuditOperation.RecordWithdrawalFromOtherChannel => "Başka kanaldan gelen caymayı kaydetti",
        AuditOperation.RecordLateDeliveryTerminationFromOtherChannel => "Başka kanaldan gelen geç teslim fesih bildirimini kaydetti",
        AuditOperation.CorrectContactEmail => "İletişim e-postasını düzeltti",
        AuditOperation.RetryCardRefund => "Kart iadesini yeniden denedi",
        AuditOperation.RefundByBankTransfer => "İadeyi havaleyle yaptı",
        AuditOperation.MarkCardRefundNotCompleted => "Kart iadesini tamamlanmadı işaretledi",
        AuditOperation.ReportRefundTransferNotCompleted => "İade havalesinin tamamlanmadığını bildirdi",
        AuditOperation.ResendEmail => "E-postayı yeniden gönderdi",
        AuditOperation.SendAdminInvitation => "Yönetici daveti gönderdi",
        AuditOperation.WithdrawAdminInvitation => "Yönetici davetini geri çekti",
        AuditOperation.CreateAdminAccount => "Yönetici hesabı açtı",
        AuditOperation.RemoveAdminAccount => "Yönetici hesabını kaldırdı",
        AuditOperation.ChangeAdminEmail => "Yönetici e-postasını değiştirdi",
        AuditOperation.PublishLegalText => "Yasal metni yayına aldı",
        AuditOperation.UpdateSettings => "Ayarı güncelledi",
        AuditOperation.ExportOrders => "Siparişleri dışa aktardı",
        AuditOperation.ExportCustomers => "Üye listesini dışa aktardı",
        AuditOperation.ExportContactRequests => "İletişim taleplerini dışa aktardı",
        AuditOperation.DeleteCustomerAccount => "Müşteri hesabını sildi",
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Every trail operation has a Turkish name."),
    };
}
