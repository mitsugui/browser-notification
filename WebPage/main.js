document.getElementById('subscribe').addEventListener('click', async () => {
    // Verificar se o navegador suporta Service Workers e Push API
    if ('serviceWorker' in navigator && 'PushManager' in window) {
      try {
        // Registrar o Service Worker
        const registration = await navigator.serviceWorker.register('service-worker.js');
  
        // Solicitar permissão para exibir notificações
        const permission = await Notification.requestPermission();
        if (permission !== 'granted') {
          throw new Error('Permissão de notificação negada');
        }
  
        // Inscrever-se para receber notificações push
        const vapidPublicKey = 'BCzmXeLee75R_ZEcErFz_ME5xICE1aA3sS2dkWMthmHt87ZsIyaONjW4_dny56pL6CAYbL5xufLlqwXlTs_TEwQ';
        const vapidPublicKeyUint8Array = urlBase64ToUint8Array(vapidPublicKey);
        const subscription = await registration.pushManager.subscribe({
          userVisibleOnly: true,
          applicationServerKey: vapidPublicKeyUint8Array
        });

        const serviceSubscription = {
          endpoint: subscription.endpoint,
          p256dh: arrayBufferToBase64url(subscription.getKey("p256dh")),
          auth: arrayBufferToBase64url(subscription.getKey("auth")),
          subject: document.getElementById("subject").value
        };

        // Enviar a inscrição para o seu servidor
        const response = await fetch('https://notificacao-windows.azurewebsites.net/subscribe', {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(serviceSubscription)
        });
  
        if (!response.ok) {
          throw new Error('Erro ao se inscrever para notificações push');
        }
  
        alert('Inscrição bem-sucedida!');
      } catch (error) {
        console.error('Erro:', error);
        alert('Erro ao se inscrever para notificações push');
      }
    } else {
      alert('Seu navegador não suporta notificações push');
    }
  });

function urlBase64ToUint8Array(base64String) {
  const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
  const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
  const rawData = atob(base64);
  const outputArray = new Uint8Array(rawData.length);
  for (let i = 0; i < rawData.length; ++i) {
    outputArray[i] = rawData.charCodeAt(i);
  }
  return outputArray;
}

function arrayBufferToBase64url(buffer) {
  var bytes = new Uint8Array(buffer);
  var binaryString = String.fromCharCode.apply(null, bytes);
  const base64String = btoa(binaryString);
  
  // Converter string Base64 para URL Base64
  const urlBase64String = base64String
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '');

  return urlBase64String;
}