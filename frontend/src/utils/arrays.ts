export function copy<T>(dst: T[], src: T[]) {
  mapCopy(dst, src, (val) => val);
}

export function mapCopy<A, B>(dst: B[], src: A[], map: (val: A) => B) {
  for (const val of src) {
    dst.push(map(val));
  }
}
