import argparse
import cPickle
import os
import platform
from random import random


class Point:
    def __init__(self, args):
        if len(args) >= 3:
            self.x = float(args[0])
            self.y = float(args[1])
            self.z = float(args[2])
        if len(args) >= 4:
            self.size = float(args[3])
        else:
            self.size = None


class Points:
    def __init__(self, indextop, thumbtop, tasksource, tasktarget):
        self.indextop = indextop
        self.thumbtop = thumbtop
        self.tasksource = tasksource
        self.tasktarget = tasktarget


def read_file_raw(file_name):
    global ty
    f = open(file_name, 'r')
    items = []
    while True:
        line = f.readline()
        if len(line) == 0:
            break
        line = line[:-2]
        arr = line.split(',')
        timenow_arr = arr[0].split(':')
        timenow = float(timenow_arr[0])*3600 + float(timenow_arr[1])*60 + float(timenow_arr[2])
        task, phase = arr[1:3]
        indextop = Point(arr[3:6])
        thumbtop = Point(arr[6:9])
        tasksource = Point(arr[9:13])
        tasktarget = Point(arr[13:17])
        points = Points(indextop, thumbtop, tasksource, tasktarget)
        items.append( (timenow, task, phase, points) )
    return items


def remove_toregister(items):
    IDLE_BREAK_THRESHOLD = 5.0
    idle_start_time = -1
    idle_break = False
    new_items = []
    tmp_items = []
    prev_phase = ''
    for item in items:
        timenow, task, phase, points = item
        if phase in ['changetask', 'changescale']:
            continue

        if phase == 'idle':
            tmp_items.append(item)
            if idle_start_time < 0:
                idle_start_time = timenow
            continue

        if phase == 'toregister':
            if prev_phase == 'idle':
                idle_break = True
            continue

        if idle_start_time > 0 and timenow - idle_start_time > IDLE_BREAK_THRESHOLD:
            idle_break = True

        if idle_break:
            tmp_items = [(timenow - random() * 2, task, 'idle', points)]

        new_items.extend(tmp_items)
        new_items.append(item)
        idle_start_time = -1
        idle_break = False
        tmp_items = []
        prev_phase = phase

    return new_items


def simplified(items):
    prev_task = ''
    prev_phase = ''
    new_items = []
    for item in items:
        timenow, task, phase, points = item
        if task != prev_task or phase != prev_phase:
            new_items.append(item)
        prev_task = task
        prev_phase = phase
    return new_items


def add_name(items, file_name):
    arr = file_name.split('-')
    username = arr[1]
    st = arr[2]
    new_items = []
    prev_task = ''
    times = {}
    times['click'] = times['drag'] = times['zoom'] = -1
    nfname = {}
    nfname['n'] = 'nofeedback'
    nfname['f'] = 'feedback'
    for item in items:
        timenow, task, phase, points = item
        if task != prev_task:
            times[task] += 1
        item = (username, nfname[st[times[task]]]) + item
        new_items.append(item)
        prev_task = task
    return new_items


def write_file(f, items):
    def write_point(p, end_comma=True):
        f.write(str(p.x) + ',' + str(p.y) + ',' + str(p.z))
        if p.size is not None:
            f.write(',' + str(p.size))
        if end_comma:
            f.write(',')

    for item in items:
        username, feedback, timenow, task, phase, points = item
        f.write(username + ',')
        f.write(feedback + ',')
        f.write(str(timenow) + ',')
        f.write(task + ',')
        f.write(phase + ',')
        write_point(points.indextop)
        write_point(points.thumbtop)
        write_point(points.tasksource)
        write_point(points.tasktarget, False)
        f.write('\n')
    f.close()


def main():
    file_names = os.listdir('data-raw/')
    print('OS:', platform.system())
    print('Files:')
    items_all = []
    for file_name in file_names:
        print('\t' + file_name)
        items = read_file_raw('data-raw/' + file_name)
        items = remove_toregister(items)
        items = simplified(items)
        items = add_name(items, file_name)
        write_file(open('merge.txt', 'a'), items)
        items_all.extend(items)
    cPickle.dump(items_all, open('merge', 'w'))

if __name__ == '__main__':
    main()
