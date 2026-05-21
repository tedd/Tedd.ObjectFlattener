import sys

with open('.github/workflows/publish-nugets.yml', 'r') as f:
    content = f.read()

content = content.replace("jobs:", "env:\n  FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: true\n\njobs:")

with open('.github/workflows/publish-nugets.yml', 'w') as f:
    f.write(content)
