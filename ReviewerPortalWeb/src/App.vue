<script setup>
import { ref } from 'vue'
import RegisterUser from './components/RegisterUser.vue'
import InviteReviewer from './components/InviteReviewer.vue'

const tab = ref('register')
const toasts = ref([])

function pushToast(msg) {
  const id = Math.random()
  toasts.value.push({ id, msg })
  setTimeout(() => {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }, 3200)
}

function onRegistered(result) {
  pushToast(`Reviewer #${result.userId} registered.`)
}
</script>

<template>
  <div class="app">
    <header class="topbar">
      <div class="brand">
        <div class="brand-mark">R</div>
        <div class="brand-name">Reviewer <em>Portal</em></div>
      </div>
      <nav class="tabs" role="tablist">
        <button role="tab" :class="['tab', { active: tab === 'register' }]" @click="tab = 'register'">
          <span class="tab-num">01</span>
          Register
        </button>
        <button role="tab" :class="['tab', { active: tab === 'invite' }]" @click="tab = 'invite'">
          <span class="tab-num">02</span>
          Invite
        </button>
      </nav>
      <div class="user-pill">v0.1 · localhost:5173</div>
    </header>

    <main class="workspace">
      <RegisterUser v-if="tab === 'register'" @registered="onRegistered" />
      <InviteReviewer v-if="tab === 'invite'" />
    </main>

    <div class="toasts">
      <div v-for="t in toasts" :key="t.id" class="toast">
        <svg width="14" height="14" viewBox="0 0 16 16" fill="none">
          <path d="M3.5 8.5L6.5 11.5L12.5 5" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" />
        </svg>
        {{ t.msg }}
      </div>
    </div>
  </div>
</template>

<style>
/* ── Design tokens ── */
:root {
  --bg: oklch(98% 0.006 85);
  --surface: oklch(100% 0 0);
  --surface-2: oklch(96.5% 0.008 85);
  --ink: oklch(22% 0.02 250);
  --ink-2: oklch(38% 0.02 250);
  --muted: oklch(56% 0.015 250);
  --faint: oklch(72% 0.012 250);
  --border: oklch(91% 0.008 250);
  --border-strong: oklch(82% 0.012 250);
  --accent: oklch(45% 0.085 195);
  --success: oklch(52% 0.13 155);
  --success-soft: oklch(95% 0.04 155);
  --danger: oklch(55% 0.18 25);
  --danger-soft: oklch(96% 0.025 25);
  --warning-soft: oklch(96% 0.035 75);

  --radius: 10px;
  --radius-lg: 14px;

  --shadow-sm: 0 1px 0 oklch(20% 0.02 250 / 0.04), 0 1px 2px oklch(20% 0.02 250 / 0.05);
  --shadow: 0 1px 0 oklch(20% 0.02 250 / 0.04), 0 6px 18px -8px oklch(20% 0.02 250 / 0.12);

  --font-sans: "Geist", ui-sans-serif, system-ui, -apple-system, "Segoe UI", sans-serif;
  --font-serif: "Newsreader", ui-serif, Georgia, serif;
  --font-mono: "Geist Mono", ui-monospace, "SF Mono", Menlo, monospace;
}

*,
*::before,
*::after { box-sizing: border-box; }
html, body { margin: 0; padding: 0; }
body {
  font-family: var(--font-sans);
  background: var(--bg);
  color: var(--ink);
  font-size: 14px;
  line-height: 1.5;
  -webkit-font-smoothing: antialiased;
}
button { font: inherit; color: inherit; cursor: pointer; border: 0; background: transparent; }
input { font: inherit; color: inherit; }
input:focus { outline: none; }

/* ── App shell ── */
.app {
  min-height: 100vh;
  display: grid;
  grid-template-rows: auto 1fr;
}

.topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 32px;
  border-bottom: 1px solid var(--border);
  background: var(--bg);
  position: sticky;
  top: 0;
  z-index: 10;
}
.brand { display: flex; align-items: center; gap: 10px; }
.brand-mark {
  width: 28px;
  height: 28px;
  border-radius: 7px;
  background: var(--ink);
  display: grid;
  place-items: center;
  color: var(--bg);
  font-family: var(--font-serif);
  font-style: italic;
  font-weight: 500;
  font-size: 17px;
  letter-spacing: -0.02em;
}
.brand-name {
  font-family: var(--font-serif);
  font-size: 18px;
  font-weight: 500;
  letter-spacing: -0.01em;
}
.brand-name em { font-style: italic; color: var(--muted); font-weight: 400; }

.tabs {
  display: flex;
  gap: 4px;
  background: var(--surface-2);
  padding: 4px;
  border-radius: 10px;
  border: 1px solid var(--border);
}
.tab {
  padding: 7px 16px;
  border-radius: 7px;
  font-size: 13px;
  font-weight: 500;
  color: var(--muted);
  display: flex;
  align-items: center;
  gap: 8px;
  transition: color 0.15s, background 0.15s;
}
.tab:hover { color: var(--ink); }
.tab.active {
  background: var(--surface);
  color: var(--ink);
  box-shadow: var(--shadow-sm);
}
.tab .tab-num {
  font-family: var(--font-mono);
  font-size: 10px;
  color: var(--faint);
}
.tab.active .tab-num { color: var(--muted); }

.user-pill {
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--muted);
  letter-spacing: 0.02em;
}

/* ── Workspace ── */
.workspace {
  display: block;
  padding: 48px 32px;
  max-width: 640px;
  margin: 0 auto;
  width: 100%;
}
@media (max-width: 700px) {
  .workspace { padding: 24px 16px; }
  .topbar { padding: 12px 16px; }
  .user-pill { display: none; }
}

/* ── Panel header ── */
.panel-header {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  margin-bottom: 24px;
}
.panel-title {
  font-family: var(--font-serif);
  font-weight: 500;
  font-size: 28px;
  letter-spacing: -0.02em;
  margin: 0;
}
.panel-sub {
  color: var(--muted);
  font-size: 13px;
  margin-top: 4px;
}
.panel-step {
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--faint);
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

/* ── Card ── */
.card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}
.card-body { padding: 28px; }
.card-section {
  padding: 20px 28px;
  border-top: 1px solid var(--border);
}

/* ── Form ── */
.field {
  display: block;
  margin-bottom: 20px;
}
.field-label {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}
.field-label-text {
  font-size: 13px;
  font-weight: 500;
  color: var(--ink-2);
}
.field-error-hint {
  font-size: 12px;
  color: var(--danger);
}
.input-wrap {
  position: relative;
  display: flex;
  align-items: center;
}
.input {
  width: 100%;
  padding: 11px 14px;
  border: 1px solid var(--border-strong);
  border-radius: var(--radius);
  background: var(--surface);
  font-size: 14px;
  transition: border-color 0.15s, box-shadow 0.15s;
}
.input:focus {
  border-color: var(--accent);
  box-shadow: 0 0 0 3px oklch(45% 0.085 195 / 0.12);
}
.input.has-error { border-color: var(--danger); }
.input.has-error:focus { box-shadow: 0 0 0 3px oklch(55% 0.18 25 / 0.12); }
.input-prefix {
  position: absolute;
  left: 12px;
  font-family: var(--font-mono);
  color: var(--faint);
  font-size: 13px;
  pointer-events: none;
}
.input.with-prefix { padding-left: 28px; }

/* ── Buttons ── */
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 10px 18px;
  border-radius: var(--radius);
  font-size: 13px;
  font-weight: 500;
  transition: background 0.15s, color 0.15s, border-color 0.15s, transform 0.05s;
  border: 1px solid transparent;
  white-space: nowrap;
}
.btn:active { transform: translateY(1px); }
.btn-primary { background: var(--ink); color: var(--bg); }
.btn-primary:hover { background: oklch(15% 0.02 250); }
.btn-primary:disabled { background: oklch(78% 0.01 250); cursor: not-allowed; transform: none; }
.btn-ghost {
  color: var(--ink-2);
  border-color: var(--border-strong);
  background: var(--surface);
}
.btn-ghost:hover { background: var(--surface-2); }

/* ── Result card ── */
.result { animation: fade-up 0.35s ease-out; }
@keyframes fade-up {
  from { opacity: 0; transform: translateY(6px); }
  to { opacity: 1; transform: translateY(0); }
}
.result-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 18px 24px;
  border-bottom: 1px solid var(--border);
}
.result-icon {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: grid;
  place-items: center;
  flex-shrink: 0;
}
.result-icon.ok { background: var(--success-soft); color: var(--success); }
.result-title {
  font-family: var(--font-serif);
  font-size: 20px;
  font-weight: 500;
  letter-spacing: -0.01em;
}

/* ── Misc ── */
.result-header-text { flex: 1; }

.card-section-footer {
  background: var(--surface-2);
  border-bottom-left-radius: var(--radius-lg);
  border-bottom-right-radius: var(--radius-lg);
}

.server-error {
  padding: 10px 14px;
  background: var(--danger-soft);
  color: var(--danger);
  border-radius: var(--radius);
  font-size: 13px;
  border: 1px solid oklch(85% 0.06 25);
}

.spinner {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 1.5px solid currentColor;
  border-top-color: transparent;
  animation: spin 0.7s linear infinite;
  display: inline-block;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* ── Toasts ── */
.toasts {
  position: fixed;
  bottom: 24px;
  right: 24px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  z-index: 30;
}
.toast {
  background: var(--ink);
  color: var(--bg);
  padding: 10px 14px;
  border-radius: var(--radius);
  font-size: 13px;
  box-shadow: var(--shadow);
  animation: toast-in 0.25s ease-out;
  display: flex;
  align-items: center;
  gap: 10px;
}
@keyframes toast-in {
  from { opacity: 0; transform: translateY(6px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
