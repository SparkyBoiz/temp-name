import os
import re

root = os.path.join(os.path.dirname(__file__), '..', 'Assets', 'Scripts')
root = os.path.normpath(root)

pattern = re.compile(r"/\*([\s\S]*?)\*/|//.*?$", re.MULTILINE)

changed_files = []
for dirpath, dirnames, filenames in os.walk(root):
    for fname in filenames:
        if fname.endswith('.cs'):
            path = os.path.join(dirpath, fname)
            with open(path, 'r', encoding='utf-8') as f:
                text = f.read()
            new_text = pattern.sub('', text)
            # Remove any leftover Windows carriage returns inconsistently
            new_text = new_text.replace('\r\n', '\n')
            # Write back using original newline style (LF)
            if new_text != text:
                with open(path, 'w', encoding='utf-8', newline='\n') as f:
                    f.write(new_text)
                changed_files.append(os.path.relpath(path, os.path.dirname(root)))

print('Removed comments from {} files.'.format(len(changed_files)))
for p in changed_files:
    print(p)
