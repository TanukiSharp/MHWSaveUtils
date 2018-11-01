# Amendment of the license

DO NOT use this code, in part or in totality, to cheat, or produce code that would eventually lead to cheat.

# Overview

This library contains utility code to find, decrypt and read save data for Monster Hunter: World.

The purpose of this project to factorize code across different projects.

- [Armory](https://github.com/TanukiSharp/MHArmory) an armor set search application
- [MHWWeaponUsage](https://github.com/TanukiSharp/MHWWeaponUsage) a tool that display your weapon usage

# Interesting research

I've implemented an unsafe decryption method, but surprisingly it is as fast as the managed decryption method.

I was expecting it to be much fast since it does in-place decryption without tons of unnecessary copies. Also, parallelizing the unsafe method does not make it faster either.

Maybe the blowfish algorithm itself takes too much overhead for such optimizations to end up being insignificant.

# Thanks

Special thanks to @Nexusphobiker from who I used the decrypt function, the very base for all other things.
https://github.com/Nexusphobiker/MHWSaveDecrypter

v00d00y, Asterisk
