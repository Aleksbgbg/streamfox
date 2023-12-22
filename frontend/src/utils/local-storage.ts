export function localGetFloat(name: string, defaultValue: number): number {
  return Number.parseFloat(localGet(name, defaultValue.toString()));
}

export function localGetBool(name: string, defaultValue: boolean): boolean {
  return localGet(name, defaultValue.toString()).trim().toLowerCase() === "true";
}

export function localGet(name: string, defaultValue: string): string {
  return localStorage.getItem(name) ?? defaultValue;
}

export function localSet(name: string, value: string) {
  return localStorage.setItem(name, value);
}
