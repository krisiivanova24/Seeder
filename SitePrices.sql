create table Price(
name varchar(500) not null,
url varchar(20000) not null,
site varchar(150) not null,
price varchar(150) not null,
date varchar(150) not null
);

create table info(
id int not null,
timerinfo varchar(10) not null,
hasnewupdate varchar(10) not null,
UpdateURL varchar(1000) not null
);

insert into info (id,timerinfo,hasnewupdate,UpdateURL) 
values ('1','1','false','no');

insert into Price (name,url,site,price,date) 
values ('1асд','https://www.kamko.bg/produkt/matraci-i-ramki/dvulicevi-matraci/%d0%b4%d0%b2%d1%83%d0%bb%d0%b8%d1%86%d0%b5%d0%b2-%d0%bc%d0%b0%d1%82%d1%80%d0%b0%d0%ba-%d1%82%d0%b5%d0%bc%d0%bf%d0%be/','Kamkobg','150лв.','2020-07-18');

UPDATE info SET UpdateURL='https://dox.abv.bg/download?id=bd8dfd37ac', hasnewupdate='false' WHERE id='1';

DELETE FROM Price
WHERE url = 'https://www.kamko.bg/produkt/matraci-i-ramki/dvulicevi-matraci/%d0%b4%d0%b2%d1%83%d0%bb%d0%b8%d1%86%d0%b5%d0%b2-%d0%bc%d0%b0%d1%82%d1%80%d0%b0%d0%ba-%d1%82%d0%b5%d0%bc%d0%bf%d0%be/';

create table comp(
lastlogin varchar(50),
ramuse varchar(50),
lastcheck varchar(50)
);