OS =  $(shell uname)
USER =  $(shell whoami)
ARCH = $(shell arch)
MAKE = gmake # Change this to make for debian systems 
CC = ../bin/gmcs
RUN = ../bin/mono
CFLAGS = -warn:4 -unsafe -r:System.Drawing 
RUNFLAGS = --aot -O=all
DEBUG = -define:DEBUG
OPENGL = -define:OpenGl -define:ScreenShot -lib:~ -r:Tao.FreeGlut.dll -r:Tao.OpenGl.dll
LIBS =

BINDIR = /usr/local/bin
BINNAME = mai.bin
INSTALL = /usr/bin/install -m 644

SRCS = -recurse:'./*.cs'
CLEANFILES = $(BINNAME) $(BINNAME).so

all:
	@echo -e "Attempting Build for ${OS}\r\n"
	$(MAKE) ${OS}
	@echo -e "\r\nThe Build was Successful for ${OS}"
hsh:
	$(CC) $(CFLAGS) $(SRCS) -out:$(BINNAME)
opengl:
	$(CC) $(CFLAGS) $(OPENGL) $(SRCS) -out:$(BINNAME)
debug: 	
	$(CC) $(CFLAGS) $(DEBUG) $(OPENGL) $(SRCS) -out:$(BINNAME)
optimize:
	$(RUN) $(RUNFLAGS) $(BINNAME)
Linux: opengl optimize
Darwin: hsh 
install:
	$(MAKE)
	cp $(BINNAME) $(BINDIR)
	chmod +x $(BINDIR)/$(BINNAME)
	$(MAKE) clean
uninstall:
	rm  $(BINDIR)/$(BINNAME)
	$(MAKE) clean
clean:
	rm -f ${CLEANFILES}
todo:
	grep -nH TODO ${SRCS} ${HDRS} 
linecount:
	wc -l */*.cs */*/*.cs */*/*/*.cs
