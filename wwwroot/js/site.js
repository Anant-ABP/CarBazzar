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

  // Simple client-side chat append
  function setupChat() {
    const thread = document.getElementById("chatThread");
    const input = document.getElementById("chatInput");
    const sendBtn = document.getElementById("chatSend");
    if (!thread || !input || !sendBtn) return;

    function appendMessage(text) {
      const trimmed = text.trim();
      if (!trimmed) return;
      const bubble = document.createElement("div");
      bubble.className = "bubble right";
      bubble.textContent = trimmed;

      const t = document.createElement("div");
      t.className = "t";
      t.textContent = new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
      bubble.appendChild(t);

      thread.appendChild(bubble);
      thread.scrollTop = thread.scrollHeight;
      input.value = "";
    }

    sendBtn.addEventListener("click", () => appendMessage(input.value));
    input.addEventListener("keydown", (e) => {
      if (e.key === "Enter") {
        e.preventDefault();
        appendMessage(input.value);
      }
    });
  }

  window.addEventListener("DOMContentLoaded", setupChat);
})();
