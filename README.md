```
apiVersion: apps/v1
kind: DaemonSet
metadata:
  labels:
    app: memory-exporter-api
  name: memory-exporter-api
  namespace: monitoring
spec:
  selector:
    matchLabels:
      app: memory-exporter-api
  
  template:
    metadata:
      labels:
        app: memory-exporter-api
        metrics: enabled

    spec:    
      securityContext:
        windowsOptions:
          runAsUserName: "NT AUTHORITY\\system"
      #nodeSelector:
      #  kubernetes.io/os: windows
      #  kubernetes.io/arch: amd64
      #  role: prod-be-api-win

      containers:
      - name: memory-exporter-api
        image: mentalistdev/monitoring-memory:1.1.1-win-ltsc2019
        imagePullPolicy: IfNotPresent
        ports:
        - name: http
          containerPort: 80
          hostPort: 80
        livenessProbe:
          httpGet:
            path: /status
            port: 80
          initialDelaySeconds: 5
          timeoutSeconds: 5
```