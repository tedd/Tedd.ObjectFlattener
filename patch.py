import sys

with open('src/Tedd.ObjectFlattener/ObjectFlattener.cs', 'r') as f:
    content = f.read()

content = content.replace('JToken nextNode = currentObj[currentPart];', 'JToken? nextNode = currentObj[currentPart];')

with open('src/Tedd.ObjectFlattener/ObjectFlattener.cs', 'w') as f:
    f.write(content)
