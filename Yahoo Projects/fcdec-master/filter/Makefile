
BUILD_CFLAGS=-O2
BUILD_LIBS=-lm

filter: main.o fastfir.o fft.o
	g++ -o $@ main.o fastfir.o fft.o $(BUILD_LIBS)

clean:
	rm -f *.o filter

%.o: %.c
	gcc -Wall -o $@ -c $< ${BUILD_CFLAGS}

%.o: %.cpp
	g++ -Wall -o $@ -c $< ${BUILD_CFLAGS}
