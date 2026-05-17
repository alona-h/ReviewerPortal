<script setup>
import { ref } from 'vue'
import { inviteReviewer, extractErrorMessage } from '../services/api.js'

const userId = ref('')
const submitting = ref(false)
const result = ref(null)
const apiError = ref(null)

function reset() {
  result.value = null
  apiError.value = null
  userId.value = ''
}

function onUserIdInput(e) {
  userId.value = e.target.value.replace(/[^0-9]/g, '')
  result.value = null
  apiError.value = null
}

async function doSubmit() {
  const id = userId.value
  if (!id) { apiError.value = 'Enter a reviewer ID.'; return }

  submitting.value = true
  result.value = null
  apiError.value = null

  try {
    result.value = await inviteReviewer(Number(id))
  } catch (err) {
    apiError.value = extractErrorMessage(err)
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div>
    <div class="panel-header">
      <div>
        <div class="panel-step">Step 02 · Invite</div>
        <h1 class="panel-title">Send a review invitation</h1>
        <p class="panel-sub">Enter a reviewer ID to send them an invitation.</p>
      </div>
    </div>

    <div class="card">
      <form class="card-body" @submit.prevent="doSubmit" novalidate>
        <div class="field">
          <div class="field-label">
            <span class="field-label-text">Reviewer ID</span>
          </div>
          <div class="input-with-action">
            <div class="input-wrap">
              <span class="input-prefix">#</span>
              <input
                class="input with-prefix"
                placeholder="e.g. 101"
                :value="userId"
                @input="onUserIdInput"
                autocomplete="off"
              />
            </div>
            <button type="submit" class="btn btn-primary" :disabled="submitting">
              <span v-if="submitting" class="spinner" />
              {{ submitting ? 'Checking…' : 'Check & invite' }}
            </button>
          </div>
        </div>

        <div v-if="apiError" class="server-error">
          {{ apiError }}
        </div>
      </form>
    </div>

    <div v-if="result" class="card result result-card">
      <div class="result-header">
        <div :class="['result-icon', result.success ? 'ok' : 'warn']">
          <svg v-if="result.success" width="16" height="16" viewBox="0 0 16 16" fill="none">
            <path d="M3.5 8.5L6.5 11.5L12.5 5" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
          <svg v-else width="16" height="16" viewBox="0 0 16 16" fill="none">
            <path d="M4 4L12 12M12 4L4 12" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" />
          </svg>
        </div>
        <div class="result-header-text">
          <div class="result-title">{{ result.message }}</div>
        </div>
      </div>

      <div class="card-section card-section-footer flex-between">
        <div class="muted">
          {{ result.success
            ? 'Invitation queued. The reviewer will receive an email shortly.'
            : 'No invitation was sent. The reviewer does not meet the current eligibility thresholds.'
          }}
        </div>
        <button class="btn btn-ghost" @click="reset">Check another</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.input-with-action { display: grid; grid-template-columns: 1fr auto; gap: 10px; }
.result-card { margin-top: 20px; }
.flex-between { display: flex; align-items: center; justify-content: space-between; }
.result-icon.warn { background: var(--warning-soft); color: oklch(45% 0.12 75); }
.muted { color: var(--muted); font-size: 12px; }
</style>
