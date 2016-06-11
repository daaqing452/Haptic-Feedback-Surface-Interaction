import cPickle
import math


class Point:
    pass

class Points:
    pass

items = cPickle.load(open('merge', 'r'))

prev_username = ''
prev_feedback = ''
prev_task = ''
prev_phase = ''
prev_tasktarget = None

prev_time_finish = -1
prev_time_touch = -1
prev_time_idle = -1
time_idle = 0
time_work = 0
breaks = 0
start = False

new_items = []

def get_dist(p0, p1):
    if p0 is None:
        p0 = p1
    return math.sqrt(math.pow(p0.x - p1.x, 2) + math.pow(p0.y - p1.y, 2))

for item in items:
    username, feedback, timenow, task, phase, points = item
    # print(task + ',' + phase)

    if username == prev_username and feedback == prev_feedback and task == prev_task:
        pass
    else:
        prev_time_finish = -1
        prev_time_touch = -1
        prev_time_idle = -1
        time_idle = 0
        time_work = 0
        breaks = 0
        start = False

    if task == 'click':
        if phase == 'idle':
            if prev_time_finish < 0 or prev_phase == 'finish':
                prev_time_finish = timenow
        elif phase == 'finish':
            dist = get_dist(prev_tasktarget, points.tasktarget)
            item = item + (timenow - prev_time_finish, 0, 0, 1, dist)
            new_items.append(item)
            prev_tasktarget = points.tasktarget

    else:
        if phase == "idle":
            if start:
                breaks += 1
            prev_time_idle = timenow
        elif phase == 'touch':
            if not start:
                dist = get_dist(points.tasksource, points.tasktarget)
            start = True
            if prev_time_idle > 0:
                time_idle += timenow - prev_time_idle
            prev_time_touch = timenow
            pass
        elif phase == 'finish':
            time_work += timenow - prev_time_touch
            item = item + (time_idle + time_work, time_idle, time_work, breaks + 1, dist)
            new_items.append(item)
            prev_time_touch = -1
            prev_time_idle = -1
            time_idle = 0
            time_work = 0
            breaks = 0
            start = False

    prev_username = username
    prev_feedback = feedback
    prev_task = task
    prev_phase = phase


def write_point(p, end_comma=True):
    f.write(str(p.x) + ',' + str(p.y) + ',' + str(p.z))
    if p.size is not None:
        f.write(',' + str(p.size))
    if end_comma:
        f.write(',')

# add session
SESSION = 5
summ = {}
for item in new_items:
    username, feedback, timenow, task, phase, points, time_finish, time_idle, time_work, breaks, dist = item
    s = username + feedback + task
    if s not in summ.keys():
        summ[s] = 0
    summ[s] += 1
cntt = {}
for i in xrange(len(new_items)):
    username, feedback, timenow, task, phase, points, time_finish, time_idle, time_work, breaks, dist = new_items[i]
    s = username + feedback + task
    if s not in cntt.keys():
        cntt[s] = 0 
    new_items[i] += (cntt[s] * SESSION / summ[s], 0)[:-1]
    cntt[s] += 1


f = open('v0.csv', 'w')
f.write('username,F or N,task,ix,iy,iz,tx,ty,tz,tsx,tsy,tsz,tss,ttx,tty,ttz,tts,timetotal,timeidle,timework,breaks,dist,session\n')
for item in new_items:
    username, feedback, timenow, task, phase, points, time_finish, time_idle, time_work, breaks, dist, session = item
    f.write(username + ',')
    f.write(feedback + ',')
    f.write(task + ',')
    write_point(points.indextop)
    write_point(points.thumbtop)
    write_point(points.tasksource)
    write_point(points.tasktarget)
    f.write(str(time_finish) + ',')
    f.write(str(time_idle) + ',')
    f.write(str(time_work) + ',')
    f.write(str(breaks) + ',')
    f.write(str(dist) + ',')
    f.write(str(session))
    f.write('\n')
f.close()
