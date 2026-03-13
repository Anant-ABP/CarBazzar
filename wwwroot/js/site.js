// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
  function togglePassword(btn) {
    const wrapper = btn.closest(".auth-password");
    if (!wrapper) return;
    const input = wrapper.querySelector("input");
    if (!input) return;
    input.type = input.type === "password" ? "text" : "password";
    btn.classList.toggle("is-on", input.type === "text");
  }

  document.addEventListener("click", (e) => {
    const target = e.target;
    if (!(target instanceof Element)) return;
    const btn = target.closest("[data-password-toggle]");
    if (!btn) return;
    e.preventDefault();
    togglePassword(btn);
  });
})();
