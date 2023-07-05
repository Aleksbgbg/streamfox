export function toLowerCamelCase(string: string): string {
  return (string.charAt(0).toLowerCase() + string.slice(1)).replace(" ", "");
}
