// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Hiển thị toast notification
document.addEventListener('DOMContentLoaded', function () {
    var el = document.querySelector('.custom-toast');
    if (!el) return;
    // Nếu dùng Bootstrap 5 (bootstrap.bundle đã nạp)
    try {
    var bsToast = bootstrap.Toast.getOrCreateInstance(el, { delay: 3000 });
    bsToast.show();
} catch (e) {
    // fallback: tự ẩn bằng timeout
    setTimeout(function () {
    el.classList.remove('show');
    el.style.opacity = '0';
}, 3000);
}
});
