export function once<T extends unknown[]>(func: (...args: T) => void): (...args: T) => void {
  let called = false;
  return function (...args) {
    if (called) {
      return;
    }

    called = true;
    func(...args);
  };
}
