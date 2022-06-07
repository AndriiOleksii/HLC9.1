# HLC9.1
Highload Course. Ex 9.1

1. В папці docker/ знаходиться докер компоуз, який піднімає редіс кластер
2. За допомогою зміни параметру maxmemory-policy в docker/redis.conf можна 
опробувати різні eviction strategies. Наразі вибрав volatile-ttl, який видаляє
кеш, який найближчий до експайра
3. В папці prb_wrapper знаходиться реалізація probabilistic cache на .ner core
