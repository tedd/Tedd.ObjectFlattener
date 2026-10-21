## 2023-10-21 - [ObjectFlattener Unflatten PathComparer Optimization]
**Observation:** The internal `PathComparer` used in `ObjectFlattener.Unflatten` uses `string.Split` to iterate path segments, generating O(n) array and string allocations per key comparison.
**Strategic Action:** Substituted `string.Split` with `ReadOnlySpan<char>.Slice` to perform allocation-free segment traversal, significantly reducing GC pressure.
