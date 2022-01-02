import os
from collections import defaultdict

graded = {}

with open(f'data{1:07}.txt', 'r') as file:
    lines = file.readlines();
    for i in range(len(lines)):
        graded[lines[i][lines[i].index(',') + 1:]] = []
fi = 1
while os.path.isfile(f'data{fi:07}.txt') and fi <= 100:
    with open(f'data{fi:07}.txt', 'r') as file:
        lines = file.readlines();
        for i in range(len(lines)):
            graded[lines[i][lines[i].index(',') + 1:]].append(int(lines[i].split(',')[0]))
            #if(graded[lines[i][lines[i].index(',') + 1:]][-1] == 1):
            #    print(fi)
    fi += 1
    

j = 0
sortedKeyValue = graded.items()
sortedKeyValue = sorted(sortedKeyValue, key=lambda i: sum(i[1]) / len(i[1]))
keys, values = zip(*sortedKeyValue)

for kv in sortedKeyValue:
    f = defaultdict(int)
    for v in kv[1]:
        f[v] += 1
    print(sum(kv[1]) / len(kv[1]), min(kv[1]), max(kv[1]), kv[0].strip(), sorted(f.items()))
    #print(sum(kv[1]) / len(kv[1]), min(i[1]), max(kv[1]), kv[0].strip(), ','.join(list(map(str,kv[1]))))
'''
for k in graded:
    #print(','.join(list(map(str,graded[k]))))
    print(min(graded[k]), sum(graded[k]) / len(graded[k]), max(graded[k]))
    j += 1

print(j)
'''
