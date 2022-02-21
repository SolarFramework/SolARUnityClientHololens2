#!/bin/bash

cat /etc/resolv.conf | grep nameserver | cut -d " " -f 2 | xargs -I foo sed 's/                address: 0.0.0.0/                address: foo/g' envoy-config.yaml &> envoy-config-to-win.yaml
