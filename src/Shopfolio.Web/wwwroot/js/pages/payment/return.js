// The payment return's hand-over page (07 UC-30; 08 2.1.4 item 3): the provider loads it in the payment page's frame, so the top window is taken to the order page the
// link names. The address is the server's (the key passed its form check); nothing is read from this page's own address. Without this script the link's target="_top" does it.
const link = document.querySelector('[data-js="payment-return"]');
if (link instanceof HTMLAnchorElement && window.top) {
  window.top.location.assign(link.href);
}
