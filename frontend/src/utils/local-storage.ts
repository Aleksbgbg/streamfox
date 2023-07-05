export function localGet(name: string, defaultValue: string): string {
  return localStorage.getItem(name) ?? defaultValue;
}

export function localSet(name: string, value: string) {
  return localStorage.setItem(name, value);
}
