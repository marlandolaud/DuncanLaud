/**
 * Strips every character that is not A-Za-z0-9.
 * Mirrors the server-side PersonValidator.Sanitize logic.
 */
export function sanitizeTextInput(value) {
  return typeof value === 'string' ? value.replace(/[^A-Za-z0-9]/g, '') : '';
}
