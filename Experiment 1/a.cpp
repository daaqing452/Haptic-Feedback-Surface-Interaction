#include <string>
#include <cstdio>
#include <cstdlib>
int main () {
	freopen("rec-cbllgh.txt", "r", stdin);
	freopen("rec-lgh-fn.txt", "w", stdout);
	char a[1000];
	while (scanf("%s", a) != EOF) {
		if (a[0] == '2') printf("%s\n", a);
	}
}