export function groupBy<T>(list: T[], keyGen: (T) => string): { [key: string]: T[] } {
  return list.reduce((result, obj) => ({
      ...result,
      [keyGen(obj)]: (result[keyGen(obj)] || []).concat(obj)
    }),
    {});
}

export function mapToFilter(map: Map<string, string>): { [key: string]: string; } {
  return [...map.entries()].reduce((filter, [key, value]) => (filter[key] = value, filter), {});
}

export function objectToFilter(obj: any): { [key: string]: string; } {
  return Object.keys(obj).reduce((filter, key) => (filter[key] = obj[key].filter, filter), {});
}
