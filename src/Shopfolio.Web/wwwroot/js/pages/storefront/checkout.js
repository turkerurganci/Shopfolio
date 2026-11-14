// The payment step's progressive enhancement (T539; 04 5.13.5, 5.13.30, 7.1.5.5; 07 8.2.2). Without this file the summary is recalculated by "Hesapla", the coupon is applied by "Uygula" and the
// billing form is sent with the "teslimat adresimle aynı" box (the server reads the four delivery fields when the box is ticked). With it:
//  - a change of the delivery province, the saved address, the payment method or the coupon code sends the form with the matching button (09 16.2 item 1: requestSubmit with the submitter, the same
//    handler; the answer is a whole page, so no state is kept here);
//  - the billing fields fold away while "teslimat adresimle aynı" is ticked (state in hidden; the server still reads the box, not the fields).

const form = document.querySelector('form[data-js="checkout"]');

function sendWith(submitter) {
    if (form && submitter && form.getAttribute("aria-busy") !== "true") {
        form.requestSubmit(submitter);
    }
}

if (form) {
    const recalculate = form.querySelector('[data-js="recalculate"]');
    const applyCoupon = form.querySelector('[data-js="apply-coupon"]');

    form.addEventListener("change", (event) => {
        const field = event.target;
        if (!(field instanceof HTMLInputElement || field instanceof HTMLSelectElement)) {
            return;
        }

        const name = field.name;
        if (name === "CouponCode" && field.type !== "hidden") {
            sendWith(applyCoupon);
        } else if (name === "PaymentMethod" || name === "AddressBookId" || name === "ShippingProvinceId" || name === "BillingProvinceId") {
            sendWith(recalculate);
        }
    });

    for (const box of form.querySelectorAll('input[type="checkbox"][name="IsBillingSameAsShipping"]')) {
        const fields = box.closest('[data-js="address-form"]')?.querySelector(".form-fields");
        if (!fields) {
            continue;
        }

        const show = () => {
            fields.hidden = box.checked;
        };
        show();
        box.addEventListener("change", show);
    }
}
