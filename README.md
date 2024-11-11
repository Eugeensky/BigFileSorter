# BigFileSorter

# prerequisites

- Visual studio 2022
- .net 8

## There are two console application - File Generator and Sorter
### File Generator
- Generates a file with a user-specified number of lines in the format "[Number].[String]" (example - "681.well-groomed heartfelt stitch").
- The file will contain duplicates in both the numeric and string parts..
### Sorter
- Creates a new sorted file in the same folder as the source file.
- If the source file is too large, it will create multiple temporary files according to the predefined chunk size (10_000_000 of lines) and then merge them into the resulting file.