import axios from 'axios'

const client = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000
})

export async function registerUser(payload) {
  const response = await client.post('/api/users', payload)
  return response.data
}

export async function inviteReviewer(userId) {
  const response = await client.post(`/api/users/${userId}/invitations`)
  return response.data
}

export function extractErrorMessage(error) {
  return error?.response?.data?.error ?? error?.message ?? 'An unexpected error occurred.'
}
