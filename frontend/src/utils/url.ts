export function fullUrl(relative: string): string {
  return new URL(relative, window.location.origin).href;
}
