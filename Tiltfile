# Deploy: tell Tilt what YAML to deploy
kustomize_yaml = kustomize('.')
k8s_yaml(kustomize_yaml)

# Build: tell Tilt what images to build from which directories
docker_build('tech-challenge-api', context='.', dockerfile='./TechChallenge/Dockerfile')
docker_build('tech-challenge-api-register', context='.', dockerfile='./TechChallenge.API.Register/Dockerfile')
docker_build('tech-challenge-api-update', context='.', dockerfile='./TechChallenge.API.Update/Dockerfile')
docker_build('tech-challenge-api-delete', context='.', dockerfile='./TechChallenge.API.Delete/Dockerfile')
docker_build('tech-challenge-api-consumer', context='.', dockerfile='./TechChallenge.Consumer/Dockerfile')

# Watch: tell Tilt how to connect locally (optional)
k8s_resource('tech-challenge-api', port_forwards="5001:5001", labels=["apis"])
k8s_resource('tech-challenge-api-register', port_forwards="5004:5004", labels=["apis"])
k8s_resource('tech-challenge-api-update', port_forwards="5003:5003", labels=["apis"])
k8s_resource('tech-challenge-api-delete', port_forwards="5002:5002", labels=["apis"])
k8s_resource('tech-challenge-api-consumer', port_forwards="5000:5000", labels=["consumer"])
k8s_resource('grafana', port_forwards="3000:3000", labels=["services"])
k8s_resource('prometheus', port_forwards="9091:9090", labels=["services"])
k8s_resource('rabbitmq', port_forwards=["5672:5672","15672:15672"], labels=["services"])
k8s_resource('sqlserver', port_forwards="14433:1433", labels=["services"])