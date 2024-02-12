# TerminalProgram
## *Вспомогательный софт*
Скрипт установщика написан с помощью Inno Setup Compiler 6.2.2

https://jrsoftware.org/isdl.php

Для упрощения работы с паттерном MVVM использован ReactiveUI

https://www.reactiveui.net/

Для тестирования используется xUnit

https://xunit.net/

## *Терминальная программа*
Проект написан с использованием WPF.

Платформа:
- .NET Framework до версии 1.9.1 включительно.
- .NET 7 начиная с версии 1.10.0.
- .NET 8 начиная с версии 2.3.0.

Данная программа может выступать в роли *IP* и *SerialPort* клиента. Выбор типа клиента происходит в меню настроек.

Поддерживается два типовых режима работы:
1. Обмен данными по *стандартным* протоколам, которые поддерживает .NET.
2. Обмен данными по *специальным* протоколам.

Приложение поддерживает следующие темы оформления:
1. Темная.
2. Светлая.

# Краткое описание режимов работы
## *"Без протокола"*
В поле передачи пользователь пишет данные, которые нужно отправить. В поле приема находятся данные, которые прислал сервер или внешнее устройство.

	Поддерживаются протоколы: 
	- SerialPort (UART)
	- TCP

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/9308333a-9809-48a1-82c8-4b3eab9acf23" />
</p>

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/50abe07d-ae2f-4db1-b98e-964f0fdb0021" />
</p>

## *"Modbus"*
Пользователь может взаимодействовать с выбранными регистрами Modbus в соответствующих полях. История действий отображается в таблице. 
В полях "Запрос" и "Ответ" отображается последняя транзакция. Содержимое этих пакетов выводится в байтах.

	Поддерживаются протоколы: 
	- Modbus TCP
	- Modbus RTU
 	- Modbus ASCII

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/1955b9ff-5a89-4810-8907-e71c57f11dd1" />
</p>

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/35a00b20-2f3a-448f-be2f-d4188177f3c3" />
</p>

## *"Http"*

В верхнем поле пользователь пишет http или https запрос. 

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/92bbccba-ff2d-4e4f-834c-e6885d63c7f1" />
</p>

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/10dc1aff-62d5-4406-bf4c-a6d815f5e268" />
</p>

# Индикация

Индикация приема и передачи обеспечивает визуальный контроль текущей активности. 
Это помогает не только в мониторинге обмена данных, но и улучшает опыт пользователя при взаимодействии с подключенными устройствами. 

При получении и отправке данных мигают соответствующие индикаторы.

	Используется в режимах: 
	- "Без протокола"
	- "Modbus"
 
<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/de31a90c-b252-4a95-a526-c5236249560e"/>
</p>

# Цикличный опрос

Суть этой возможности заключается в том, что через заданный промежуток времени на хост отправляется сообщение. 
Но важно помнить, что каждый режим накладывает некоторые ограничения на работу цикличного опроса.

Эта возможность доступна для следующих режимов работы:

## *"Без протокола"*

В поле ввода пользователь вводит сообщение, которое будет отправляться на хост с заданным периодом.
При этом не важно ответит ли хост или нет.

Ответ хоста можно "запаковать" в диагностические данные. 
Формат отображаемой строки настраивается с помощью выставления соответствующих галочек.

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/a2bc2a14-ee9a-4910-a340-39e85e082fbf" />
</p>

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/f9d8e90c-53b3-4885-8f33-a81059fa6d03" />
</p>

## *"Modbus"*

В режиме "Modbus" при цикличном опросе возможно использование только функций чтения. 
При этом, если хост не ответит за указанный в настройках таймаут, то опрос прекратится.

Если в качестве интерфейса связи выбран последовательный порт, то выбор между протоколами Modbus ASCII и Modbus RTU происходит в главном окне.

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/83407b72-c01a-4b0e-9b43-590eac31ce74" />
</p>

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/9da913a8-7e5a-4de1-b653-3d8036e63be6" />
</p>

# Система сохранений настроек

Все настройки можно сохранить в файле с расширением .json 

Перед подключением пользователь может выбрать из списка необходимый для работы файл с настройками или создать новый.

Настройки можно изменить в соответствующем пункте меню.

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/4eb62085-2d01-4dc4-8369-d5b4736a9646" />
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/f6c5a88b-2aa2-4a6a-912d-1d9c33c9d3e5" />
</p>

<p align="center">
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/e794d857-cebb-4337-86da-e0e587024c76" />
  <img src="https://github.com/AndreyAbdulkayumov/TerminalProgram/assets/86914394/a54abbfb-7634-4c89-9b55-21d9accd2062" />
</p>

# *Система версирования* Global.Major.Minor

*Global* - глобальная версия репозитория. До релиза это 0. Цифра меняется во время релиза и при именениях, затрагивающих значительную часть UI или внутренней логики.

*Major* - добавление нового функционала, крупные изменения.

*Minor* - исправление багов, мелкие добавления.
