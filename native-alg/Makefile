
ifdef WITH_OPENMP
  CFLAGS += -fopenmp -DWITH_OPENMP
  LDFLAGS += -fopenmp
endif

CFLAGS += -std=c99 -O3 -g -Wall -Werror -Wextra -pedantic -fPIC -I/opt/homebrew/Cellar/openssl@3/3.1.3/include -Wno-error=deprecated-declarations
# CFLAGS += -std=c99 -O3 -g -Wall -Werror -Wextra -pedantic -fPIC -I/usr/include/openssl -Wno-error=deprecated-declarations
LDFLAGS+= -L/opt/homebrew/Cellar/openssl@3/3.1.3/lib
# LDFLAGS+= -L/usr/lib/openssl
LDLIBS += -lssl -lcrypto 

CC=gcc


#all: testfastpbkdf2 libfastpbkdf2.a bench benchmulti
all: cb pbkdf2.so

cb: clean build

build: fastpbkdf2.o
pbkdf2.so:	fastpbkdf2.o 
	$(LINK.c) -shared $^ $(LDLIBS) $(LOADLIBS) -o $@

libfastpbkdf2.a: fastpbkdf2.o
	$(AR) r $@ $^

clean:
	rm -f *.o libfastpbkdf2.a testfastpbkdf2
