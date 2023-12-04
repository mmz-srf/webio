export function errorHappensBecauseColumnsArentLoaded(e: Error): boolean {
  const columnError = 'Cannot read properties of undefined (reading \'length\')';
  return e.message.includes(columnError);
}
