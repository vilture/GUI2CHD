#!/usr/bin/env python3
import sys
import os
import struct
import binascii
from pathlib import Path

def read_ccd(ccd_file):
    with open(ccd_file, 'rb') as f:
        data = f.read()
    
    # Выводим первые 16 байт файла для отладки
    print(f"Первые 16 байт файла: {binascii.hexlify(data[:16]).decode()}")
    
    # Проверяем сигнатуру CCD
    signature = data[0:9]  # [CloneCD]
    print(f"Сигнатура файла: {signature}")
    if signature != b'[CloneCD]':
        raise ValueError(f"Неверный формат CCD файла. Ожидалось '[CloneCD]', получено: {signature}")
    
    # Пропускаем заголовок до версии
    version_offset = data.find(b'Version', 9)
    if version_offset == -1:
        raise ValueError("Не найден раздел Version в CCD файле")
    
    # Читаем версию
    version_str = data[version_offset:].split(b'\r\n')[0]
    version = int(version_str.split(b'=')[1].strip())
    print(f"Версия файла: {version}")
    
    # Ищем раздел с записями
    entries_offset = data.find(b'[Entry', version_offset)
    if entries_offset == -1:
        raise ValueError("Не найден раздел [Entry] в CCD файле")
    
    # Читаем информацию о записях
    entries = []
    current_offset = entries_offset
    while True:
        # Ищем следующую запись
        next_entry = data.find(b'[Entry', current_offset + 6)
        if next_entry == -1:
            # Это последняя запись
            entry_data = data[current_offset:]
        else:
            entry_data = data[current_offset:next_entry]
        
        # Парсим информацию о записи
        point = None
        control = None
        plba = None
        
        for line in entry_data.split(b'\r\n'):
            if line.startswith(b'Point='):
                point = int(line.split(b'=')[1].strip(), 16)
            elif line.startswith(b'Control='):
                control = int(line.split(b'=')[1].strip(), 16)
            elif line.startswith(b'PLBA='):
                plba = int(line.split(b'=')[1].strip())
        
        if point is not None and control is not None and plba is not None:
            entry = {
                'point': point,
                'control': control,
                'plba': plba
            }
            print(f"Запись: point=0x{entry['point']:02x}, control=0x{entry['control']:02x}, plba={entry['plba']}")
            entries.append(entry)
        
        if next_entry == -1:
            break
        current_offset = next_entry
    
    if not entries:
        raise ValueError("Не найдено ни одной записи в CCD файле")
    
    return entries

def convert_ccd_to_iso(ccd_file, iso_file):
    print(f"Начало конвертации файла: {ccd_file}")
    
    # Получаем путь к IMG файлу
    img_file = str(Path(ccd_file).with_suffix('.img'))
    print(f"Путь к IMG файлу: {img_file}")
    
    if not os.path.exists(img_file):
        raise FileNotFoundError(f"Не найден файл образа: {img_file}")
    
    # Читаем информацию о записях из CCD
    entries = read_ccd(ccd_file)
    
    # Открываем файлы
    with open(img_file, 'rb') as img, open(iso_file, 'wb') as iso:
        # Копируем данные из IMG в ISO
        img.seek(0)
        iso.write(img.read())
    
    print(f"Конвертация завершена успешно")
    return True

def main():
    if len(sys.argv) != 3:
        print("Использование: ccd2iso.py <ccd_file> <iso_file>")
        sys.exit(1)
    
    ccd_file = sys.argv[1]
    iso_file = sys.argv[2]
    
    try:
        convert_ccd_to_iso(ccd_file, iso_file)
        print(f"Конвертация завершена: {iso_file}")
    except Exception as e:
        print(f"Ошибка: {str(e)}")
        sys.exit(1)

if __name__ == '__main__':
    main() 