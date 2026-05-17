<script setup>
import { ref } from 'vue'
import { registerUser, extractErrorMessage } from '../services/api.js'

const emit = defineEmits(['registered'])

const form = ref({ userName: '', universityName: '', numberOfPublications: 0 })
const errors = ref({})
const submitting = ref(false)
const result = ref(null)
const serverError = ref(null)

function scoreBand(score) {
  if (score == null) return 'low'
  if (score >= 80) return 'high'
  if (score >= 60) return 'mid'
  return 'low'
}

function updateField(key, value) {
  form.value[key] = value
  if (errors.value[key]) errors.value = { ...errors.value, [key]: undefined }
  if (result.value) { result.value = null }
  serverError.value = null
}

function validate() {
  const e = {}
  if (!form.value.userName) e.userName = 'Required.'
  else if (form.value.userName.length < 3) e.userName = 'Min 3 characters.'
  if (!form.value.universityName) e.universityName = 'Required.'
  else if (form.value.universityName.length < 3) e.universityName = 'Min 3 characters.'
  const pubs = Number(form.value.numberOfPublications)
  if (form.value.numberOfPublications === '') e.numberOfPublications = 'Required.'
  else if (!Number.isInteger(pubs) || pubs < 0) e.numberOfPublications = 'Must be 0 or greater.'
  return e
}

function resetForm() {
  form.value = { userName: '', universityName: '', numberOfPublications: 0 }
  result.value = null
  errors.value = {}
  serverError.value = null
}

async function submit() {
  const errs = validate()
  if (Object.keys(errs).length) { errors.value = errs; return }

  submitting.value = true
  result.value = null
  serverError.value = null

  try {
    const res = await registerUser({
      userName: form.value.userName,
      universityName: form.value.universityName,
      numberOfPublications: Number(form.value.numberOfPublications),
    })
    result.value = res
    emit('registered', res)
  } catch (err) {
    serverError.value = extractErrorMessage(err)
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div>
    <div class="panel-header">
      <div>
        <div class="panel-step">Step 01 · Register</div>
        <h1 class="panel-title">Onboard a new reviewer</h1>
        <p class="panel-sub">Fill in the reviewer's details to add them to the system.</p>
      </div>
    </div>

    <div v-if="!result" class="card">
      <form class="card-body" @submit.prevent="submit" novalidate>
        <div class="field">
          <div class="field-label">
            <span class="field-label-text">User name</span>
            <span v-if="errors.userName" class="field-error-hint">{{ errors.userName }}</span>
          </div>
          <div class="input-wrap">
            <span class="input-prefix">@</span>
            <input
              :class="['input', 'with-prefix', { 'has-error': errors.userName }]"
              placeholder="jane.doe"
              :value="form.userName"
              @input="updateField('userName', $event.target.value)"
              autocomplete="off"
            />
          </div>
        </div>

        <div class="field">
          <div class="field-label">
            <span class="field-label-text">University</span>
            <span v-if="errors.universityName" class="field-error-hint">{{ errors.universityName }}</span>
          </div>
          <input
            :class="['input', { 'has-error': errors.universityName }]"
            placeholder="University name"
            :value="form.universityName"
            @input="updateField('universityName', $event.target.value)"
            autocomplete="off"
          />
        </div>

        <div class="field">
          <div class="field-label">
            <span class="field-label-text">Number of publications</span>
            <span v-if="errors.numberOfPublications" class="field-error-hint">{{ errors.numberOfPublications }}</span>
          </div>
          <div class="input-number">
            <input
              type="number"
              min="0"
              class="input"
              :value="form.numberOfPublications"
              @input="updateField('numberOfPublications', $event.target.value === '' ? '' : Number($event.target.value))"
            />
            <div class="stepper">
              <button type="button" @click="updateField('numberOfPublications', Number(form.numberOfPublications || 0) + 1)">▲</button>
              <button type="button" @click="updateField('numberOfPublications', Math.max(0, Number(form.numberOfPublications || 0) - 1))">▼</button>
            </div>
          </div>
        </div>

        <div v-if="serverError" class="server-error">{{ serverError }}</div>

        <div class="btn-row">
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting" class="spinner" />
            {{ submitting ? 'Registering…' : 'Register reviewer' }}
            <svg v-if="!submitting" width="14" height="14" viewBox="0 0 16 16" fill="none">
              <path d="M3 8H13M13 8L9 4M13 8L9 12" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
            </svg>
          </button>
          <button type="button" class="btn btn-ghost" @click="resetForm" :disabled="submitting">Clear</button>
        </div>
      </form>
    </div>

    <div v-if="result" class="card result">
      <div class="result-header">
        <div class="result-icon ok">
          <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
            <path d="M3.5 8.5L6.5 11.5L12.5 5" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </div>
        <div class="result-header-text">
          <div class="result-title">Reviewer registered</div>
        </div>
      </div>

      <div class="user-card">
        <div class="user-card-grid cols-3">
          <div>
            <div class="kv-label">User name</div>
            <div class="kv-value mono">@{{ result.userName }}</div>
          </div>
          <div>
            <div class="kv-label">Publications</div>
            <div class="kv-value serif">{{ result.numberOfPublications }}</div>
          </div>
          <div>
            <div class="kv-label">University score</div>
            <span :class="['score-pill', scoreBand(result.university.score)]">
              {{ Number(result.university.score).toFixed(2) }}
            </span>
          </div>
        </div>
        <div class="divider" />
        <div>
          <div class="kv-label">Resolved university</div>
          <div class="kv-value serif">{{ result.university.universityName }}</div>
        </div>
      </div>

      <div class="card-section card-section-footer">
        <button class="btn btn-ghost" @click="resetForm">Register another</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.input-number {
  display: grid;
  grid-template-columns: 1fr auto;
  border: 1px solid var(--border-strong);
  border-radius: var(--radius);
  overflow: hidden;
  background: var(--surface);
}
.input-number:focus-within {
  border-color: var(--accent);
  box-shadow: 0 0 0 3px oklch(45% 0.085 195 / 0.12);
}
.input-number .input { border: 0; border-radius: 0; }
.input-number .input:focus { box-shadow: none; }
.stepper { display: flex; flex-direction: column; border-left: 1px solid var(--border); }
.stepper button { padding: 0 12px; font-size: 11px; color: var(--muted); height: 22px; display: grid; place-items: center; }
.stepper button:hover { background: var(--surface-2); color: var(--ink); }
.stepper button + button { border-top: 1px solid var(--border); }
.btn-row { display: flex; gap: 10px; align-items: center; margin-top: 8px; }
.user-card { padding: 24px; }
.user-card-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 18px 24px; }
.user-card-grid.cols-3 { grid-template-columns: repeat(3, 1fr); }
.kv-label { font-size: 11px; font-family: var(--font-mono); text-transform: uppercase; letter-spacing: 0.08em; color: var(--faint); margin-bottom: 4px; }
.kv-value { font-size: 16px; color: var(--ink); font-weight: 500; }
.kv-value.serif { font-family: var(--font-serif); font-weight: 500; font-size: 18px; letter-spacing: -0.01em; }
.kv-value.mono { font-family: var(--font-mono); font-size: 14px; font-weight: 500; }
.score-pill { display: inline-flex; align-items: baseline; gap: 4px; padding: 3px 9px 4px; border-radius: 999px; font-family: var(--font-mono); font-size: 12px; font-weight: 500; background: var(--surface-2); color: var(--ink-2); border: 1px solid var(--border); }
.score-pill.high { background: var(--success-soft); color: var(--success); border-color: transparent; }
.score-pill.mid { background: var(--warning-soft); color: oklch(45% 0.12 75); border-color: transparent; }
.score-pill.low { background: var(--surface-2); color: var(--muted); }
.divider { height: 1px; background: var(--border); margin: 18px 0; }
</style>
