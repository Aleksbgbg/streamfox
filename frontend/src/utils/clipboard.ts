export function clipboardCopy(value: string): Promise<void> {
  return navigator.clipboard.writeText(value);
}
