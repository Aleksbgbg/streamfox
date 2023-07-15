function combineUrl(...segments: string[]) {
  return [...segments].join("");
}

export function apiUrl(path: string) {
  return combineUrl(import.meta.env.VITE_API_ENDPOINT, path);
}
