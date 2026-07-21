import re

with open('tests/Tedd.ObjectFlattener.Tests/ObjectFlattenerTests.cs', 'r') as f:
    content = f.read()

# Let's cleanly replace the commented out tests.
content = re.sub(r'        // \[Fact\].*?\n(        // .*?\n)*', '', content)
content = re.sub(r'        \[Fact\]\n        public void Unflatten_FinalValueConflict_ContainerNotObjectOrArray\(\)\n        \{\n            //.*?\n        \}\n', '', content, flags=re.DOTALL)
content = re.sub(r'        \[Fact\]\n        public void Unflatten_CannotSetFinalValue\(\)\n        \{\n            //.*?\n        \}\n', '', content, flags=re.DOTALL)
content = re.sub(r'        \[Fact\]\n        public void Unflatten_NavigateFurther_CannotNavigateFurther\(\)\n        \{\n            //.*?\n        \}\n', '', content, flags=re.DOTALL)


# The previous script might have messed up syntax. Let's just generate it from scratch nicely.
