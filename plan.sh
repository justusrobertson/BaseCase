cd /cygdrive/$1/FastDownward/output
/cygdrive/$1/FastDownward/src/translate/translate.py /cygdrive/$1/Benchmarks/$2/domain.pddl /cygdrive/$1/Benchmarks/$2/prob$3.pddl
/cygdrive/$1/FastDownward/src/preprocess/preprocess < OUTPUT.SAS
/cygdrive/$1/FastDownward/src/search/downward --search "astar(cea())" < output