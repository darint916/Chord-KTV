1. Create an AWS account

2. Log in as root user

3. Navigate to the EC2 Management Console

4. Launch a new EC2 instance

5. Follow the given steps, leave mostly everything at default. Pick the instance type of Amazon Linux, m1.large (downgrade later).

6. Once the instance has been created, make sure that it is turned on and started from the Instances tab of the EC2 dashboard.

7. Find the "Connect" button that opens a window ssh-ed into the EC2 server.

8. Connect to the server via the in-browser ssh.

9. Do all basic installs (use wget first because apt-get is not installed by default. Then use "dnf" package manager/installer because its compatible with Amazon Linux. Here are some of the installs:
```
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update;   sudo apt-get install -y apt-transport-https &&   sudo apt-get update &&   sudo apt-get install -y dotnet-sdk-8.0
# Add the Microsoft package repository
sudo dnf install -y https://packages.microsoft.com/config/amazon-linux/2023/packages-microsoft-prod.rpm
# Install the .NET SDK
sudo dnf install -y dotnet-sdk-8.0
# Install NVM
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.4/install.sh | bash
# Load NVM
export NVM_DIR="$HOME/.nvm"
[ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"  # This loads nvm
# Install Node.js version 18.16.0
nvm install 18.16.0
nvm use 18.16.0
# Update the package index
sudo dnf update -y
# Install Docker
sudo dnf install -y docker
# Start Docker service
sudo systemctl start docker
# Enable Docker to start on boot
sudo systemctl enable docker
# Add your user to the docker group (you might need to log out and back in for this to take effect)
sudo usermod -aG docker $USER
```

10. Verify installs:
```
dotnet --version
node -v
npm -v
docker --version
```

11. Install git and clone repo
```
sudo dnf install -y git
git --version
git clone https://github.com/darint916/Chord-KTV.git
```

12. Install nginx
```
sudo dnf install -y nginx
nginx -v
```

13. Setup nginx
sudo nano /etc/nginx/conf.d/lyric-sync-app.conf:
```
   server {
       listen 80;
       server_name 44.201.142.173; # or your EC2 public IP

       root /home/ec2-user/Chord-KTV/frontend/lyric-sync-app/;
       index index.html;

       location / {
           try_files $uri $uri/ /index.html;
       }
   }
```

14. Reload nginx:
```
   sudo systemctl reload nginx
sudo systemctl start nginx
  sudo systemctl enable nginx
```

15. Optional (might be ignorable)
```
sudo setenforce 0
sudo chown -R ec2-user:ec2-user /home/ec2-user/Chord-KTV/frontend/lyric-sync-app
sudo chmod -R 755 /home/ec2-user/Chord-KTV/frontend/lyric-sync-app
```

16. Install Dotnet-ef
```
dotnet tool install --global dotnet-ef
```

17. May be optional
```
export ConnectionStrings__Postgres="Host=127.0.0.1;Port=30011;Database=postgres;Username=postgres;Password=postgrespw"
dotnet dev-certs https
dotnet dev-certs https --trust
```

18. Create Docker network
```
sudo docker network create chordktv-network
```

19. Run postgres db container
```
sudo docker run -d   --name my-postgres-container   --network chordktv-network   -p 30011:5432   -e POSTGRES_PASSWORD=postgrespw   -v my-pgdata:/var/lib/postgresql/data   postgres:15.4
```

20. If you need to hard delete the entire db:
```
sudo docker volume ls # (you should see my-pgdata)
sudo docker volume rm my-pgdata # Now the entire docker volume for the db is gone.
```

21. If you need to reset migrations:
Start by re-running the postgres db container (Step 19.)
Then do this:
```
rm -rf ChordKTV/Migrations
cd ChordKTV
dotnet ef migrations add InitialCreate
dotnet ef database update --connection "Host=localhost;Port=30011;Database=postgres;Username=postgres;Password=postgrespw"
cd ..
```

22. Add .ENV file to frontend like this:
```
touch Chord-KTV/frontend/lyric-sync-app/.env
nano Chord-KTV/frontend/lyric-sync-app/.env
```
Write:
```
VITE_GOOGLE_CLIENT_ID=626673404798-0kor8f3qeonhso9ea54mouqgsta3acht.apps.googleusercontent.com
VITE_API_BASE_URL=https://chordktv.com
```

23. To launch frontend and backend, make sure you are in the Chord-KTV directory (repo root). Then do this:
```
# Stop existing backend if it's already running
sudo docker stop chordktv-backend
sudo docker rm chordktv-backend

# Build frontend files and move them to wwwroot (make the dir if it doesn't exist)
cd frontend/lyric-sync-app/
npm run build
cd ../.. 
rm -rf ChordKTV/wwwroot/*
cp -r frontend/lyric-sync-app/dist/* ChordKTV/wwwroot/
ls ChordKTV/wwwroot/

# Build backend, will serve static frontend build files from wwwroot
sudo docker build -f ChordKTV/Dockerfile -t chordktv-backend . 
```

24. Final step: run the backend docker container.
The "-e"s are ENV variables. You must fill them all from the secrets file. These can also be passed in more cleanly through a .env file. Just ask chatgpt how to do it.
```
sudo docker run -d   --name chordktv-backend   --network chordktv-network   -p 5259:5259  -v /home/ec2-user/.config/gcloud:/gcloud:ro,Z   -e "GOOGLE_APPLICATION_CREDENTIALS=/gcloud/application_default_credentials.json"   -e "ASPNETCORE_ENVIRONMENT=Production"   -e "ConnectionStrings__PostgreSql=Host=my-postgres-container;Port=5432;Database=postgres;Username=postgres;Password=postgrespw"   -e "YouTube__ApiKey=AIz...P8"   -e "YouTube__SearchApiKey=AIz...P8"   -e "Genius__ApiKey=Hf8...SPi"   -e "OpenAI__ApiKey=sk-proj-h...A"        -e "Authentication__Google__ClientId=62...ht.apps.googleusercontent.com"  -e "Jwt__Key=AH...m" chordktv-backend
```

25. If the above command runs successfully, to verify that everything is nominal, run the following command:
```
sudo docker ps -a
```

If you see something like this, where the STATUS of both containers are UP, then you're done here:
```
[ec2-user@ip-172-31-88-95 ~]$ sudo docker ps -a
CONTAINER ID   IMAGE              COMMAND                  CREATED      STATUS      PORTS                                                      NAMES
3ca5c358f9e5   chordktv-backend   "dotnet ChordKTV.dll"    5 days ago   Up 3 days   0.0.0.0:5259->5259/tcp, :::5259->5259/tcp, 8080-8081/tcp   chordktv-backend
3b9fde4bb23c   postgres:15.4      "docker-entrypoint.s…"   5 days ago   Up 3 days   0.0.0.0:30011->5432/tcp, :::30011->5432/tcp                my-postgres-container
```

If you ever see that one of the containers exited (namely backend), find out why by running:
```
sudo docker logs chordktv-backend # name of container
```

To restart/update just the backend container (ignoring frontend), run:
```
sudo docker stop chordktv-backend
sudo docker rm chordktv-backend
# and now run Step 24's command.
```

To restart/update the frontend (and also backend), run steps 23 and 24.


______________

For public domain setup do this: 
1. Buy a domain (namecheap)
2. Open DNS settings for your domain
3. Change nameserver settings to use custom nameservers instead of namecheap nameservers.
4. To get your new nameservers, go to AWS Route 53 (another app in your AWS dashboard)
5. Enter Hosted Zones
6. Follow these instructions: https://medium.com/@yashpatel007/how-to-connect-your-amazon-ec2-instance-with-a-domain-name-80ad8959078
