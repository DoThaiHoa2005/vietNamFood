// File này chứa code HTML cho bản đồ tự động chuyển đổi online/offline
// Copy nội dung method BuildMapHtml này vào MapWindow.xaml.cs để thay thế method cũ

/*
HƯỚNG DẪN: Thay thế method BuildMapHtml trong MapWindow.xaml.cs bằng code dưới đây:

private string BuildMapHtml(string markersJson, double focusLat, double focusLng, double userLat, double userLng)
{
    string focusLatStr = focusLat.ToString(System.Globalization.CultureInfo.InvariantCulture);
    string focusLngStr = focusLng.ToString(System.Globalization.CultureInfo.InvariantCulture);
    string userLatStr = userLat.ToString(System.Globalization.CultureInfo.InvariantCulture);
    string userLngStr = userLng.ToString(System.Globalization.CultureInfo.InvariantCulture);
    
    return $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<meta name='viewport' content='width=device-width, initial-scale=1'/>
<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
<style>
  html,body{{margin:0;padding:0;height:100%;width:100%;font-family:Arial,sans-serif;}}
  #map,#offline-map{{position:absolute;width:100%;height:100%;}}
  #offline-map{{display:none;background:#e8f4f8;background-image:repeating-linear-gradient(0deg,transparent,transparent 50px,rgba(0,0,0,.02) 50px,rgba(0,0,0,.02) 51px),repeating-linear-gradient(90deg,transparent,transparent 50px,rgba(0,0,0,.02) 50px,rgba(0,0,0,.02) 51px);}}
  
  .offline-marker{{position:absolute;transform:translate(-50%,-50%);cursor:pointer;z-index:10;}}
  .offline-marker-icon{{width:36px;height:36px;background:#E63946;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:20px;box-shadow:0 3px 10px rgba(230,57,70,0.4);border:3px solid white;transition:transform 0.2s;}}
  .offline-marker:hover .offline-marker-icon{{transform:scale(1.2);}}
  .offline-user{{position:absolute;width:18px;height:18px;background:#2563eb;border:3px solid white;border-radius:50%;box-shadow:0 0 12px rgba(37,99,235,0.6);transform:translate(-50%,-50%);z-index:100;animation:pulse 2s infinite;}}
  @keyframes pulse{{0%,100%{{box-shadow:0 0 12px rgba(37,99,235,0.6);}}50%{{box-shadow:0 0 20px rgba(37,99,235,1);}}}}
  .offline-route{{position:absolute;border-top:3px dashed #dc2626;transform-origin:0 0;pointer-events:none;z-index:5;}}
  
  .food-popup{{font-family:sans-serif;min-width:180px;}}
  .food-popup h3{{margin:0 0 6px;color:#E63946;font-size:14px;}}
  .food-popup .rating{{color:#f59e0b;font-size:13px;}}
  .food-popup .desc{{font-size:12px;color:#555;margin-top:4px;}}
  .food-popup .btn-route{{background:#2563eb;color:white;border:none;padding:6px 12px;border-radius:4px;cursor:pointer;margin-top:8px;font-size:12px;}}
  .food-popup .btn-route:hover{{background:#1d4ed8;}}
  .user-dot{{width:18px;height:18px;background:#2563eb;border:3px solid white;border-radius:50%;box-shadow:0 0 8px rgba(37,99,235,0.6);}}
  
  .route-controls{{position:absolute;top:10px;right:10px;z-index:1000;background:white;padding:10px;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.2);}}
  .route-controls button{{display:block;width:100%;margin:5px 0;padding:8px 12px;border:none;border-radius:4px;cursor:pointer;font-size:13px;font-weight:600;}}
  .btn-stop-route{{background:#dc2626;color:white;}}
  .btn-stop-route:hover{{background:#b91c1c;}}
  .route-info{{font-size:11px;color:#666;margin-top:5px;}}
  
  .status-badge{{position:absolute;top:10px;left:10px;background:white;padding:8px 12px;border-radius:6px;box-shadow:0 2px 8px rgba(0,0,0,0.1);font-size:12px;font-weight:600;z-index:1000;}}
  .status-online{{color:#10b981;}}
  .status-offline{{color:#dc2626;}}
  #loading{{position:absolute;top:50%;left:50%;transform:translate(-50%,-50%);background:white;padding:20px;border-radius:8px;box-shadow:0 2px 10px rgba(0,0,0,0.2);z-index:2000;}}
</style>
</head>
<body>
<div id='loading'>⏳ Đang tải...</div>
<div class='status-badge'><span id='status-text'>🔄 Kiểm tra...</span></div>
<div id='map'></div>
<div id='offline-map'></div>
<div class='route-controls' id='routeControls' style='display:none'>
  <button class='btn-stop-route' onclick='stopRoute()'>🛑 Dừng</button>
  <div class='route-info' id='routeInfo'></div>
</div>
<script>
var isOnline=navigator.onLine;var map=null;var offlineMap=null;var foods={markersJson};var markers=[];var routeLine=null;var currentDestination=null;var userMarker=null;var offlineUserMarker=null;var offlineRouteElement=null;
function updateStatus(online){{var statusText=document.getElementById('status-text');if(online){{statusText.innerHTML='🟢 Online';statusText.className='status-online';}}else{{statusText.innerHTML='🔴 Offline';statusText.className='status-offline';}}}}
function initOnlineMap(){{console.log('📡 Online map');document.getElementById('map').style.display='block';document.getElementById('offline-map').style.display='none';if(!map){{map=L.map('map').setView([{focusLatStr},{focusLngStr}],15);var tileLayer=L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png',{{attribution:'© OSM',maxZoom:19}});tileLayer.addTo(map);tileLayer.on('tileerror',function(){{console.log('Tile error, switching offline');switchToOffline();}});tileLayer.on('load',function(){{document.getElementById('loading').style.display='none';}});var userIcon=L.divIcon({{html:'<div class=""user-dot""></div>',iconSize:[18,18],iconAnchor:[9,9],className:''}});userMarker=L.marker([{userLatStr},{userLngStr}],{{icon:userIcon,zIndexOffset:1000}}).addTo(map).bindPopup('📍 Vị trí của bạn');var foodIcon=L.divIcon({{html:'<div style=""background:#E63946;color:white;border-radius:50%;width:32px;height:32px;display:flex;align-items:center;justify-content:center;font-size:16px;box-shadow:0 2px 6px rgba(0,0,0,0.3);border:3px solid white"">🍜</div>',iconSize:[32,32],iconAnchor:[16,16],popupAnchor:[0,-18],className:''}});foods.forEach(function(f){{var popupContent='<div class=""food-popup""><h3>'+f.name+'</h3><div class=""rating"">⭐ '+f.rating+'</div><div class=""desc"">'+f.desc+'</div><button class=""btn-route"" onclick=""startRoute('+f.lat+','+f.lng+',\\''+f.name.replace(/'/g,'')+'\\')"">🗺️ Chỉ đường</button></div>';var m=L.marker([f.lat,f.lng],{{icon:foodIcon}}).addTo(map).bindPopup(popupContent);markers.push({{marker:m,food:f}});}});map.setView([{focusLatStr},{focusLngStr}],15);}}setTimeout(function(){{document.getElementById('loading').style.display='none';}},3000);}}
function initOfflineMap(){{console.log('📴 Offline map');document.getElementById('map').style.display='none';document.getElementById('offline-map').style.display='block';document.getElementById('loading').style.display='none';var container=document.getElementById('offline-map');container.innerHTML='';var minLat=Math.min(...foods.map(f=>f.lat),{userLatStr})-0.01;var maxLat=Math.max(...foods.map(f=>f.lat),{userLatStr})+0.01;var minLng=Math.min(...foods.map(f=>f.lng),{userLngStr})-0.01;var maxLng=Math.max(...foods.map(f=>f.lng),{userLngStr})+0.01;function getX(lng){{return((lng-minLng)/(maxLng-minLng))*100;}}function getY(lat){{return(1-(lat-minLat)/(maxLat-minLat))*100;}}var userDiv=document.createElement('div');userDiv.className='offline-user';userDiv.style.left=getX({userLngStr})+'%';userDiv.style.top=getY({userLatStr})+'%';userDiv.title='Vị trí của bạn';container.appendChild(userDiv);offlineUserMarker=userDiv;foods.forEach(function(f){{var markerDiv=document.createElement('div');markerDiv.className='offline-marker';markerDiv.style.left=getX(f.lng)+'%';markerDiv.style.top=getY(f.lat)+'%';markerDiv.innerHTML='<div class=""offline-marker-icon"">🍜</div>';markerDiv.title=f.name+' ⭐'+f.rating;markerDiv.onclick=function(){{startRoute(f.lat,f.lng,f.name);}};container.appendChild(markerDiv);}});window.offlineGetX=getX;window.offlineGetY=getY;}}
async function drawOnlineRoute(destLat,destLng,destName){{if(routeLine)map.removeLayer(routeLine);var userPos=userMarker.getLatLng();var straightDist=(map.distance(userPos,[destLat,destLng])/1000).toFixed(1);try{{var url='https://router.project-osrm.org/route/v1/driving/'+userPos.lng+','+userPos.lat+';'+destLng+','+destLat+'?overview=full&geometries=geojson';var controller=new AbortController();var timeoutId=setTimeout(function(){{controller.abort();}},5000);var response=await fetch(url,{{signal:controller.signal}});clearTimeout(timeoutId);var data=await response.json();if(data.code==='Ok'&&data.routes&&data.routes.length>0){{var coords=data.routes[0].geometry.coordinates;var latLngs=coords.map(function(c){{return[c[1],c[0]];}});routeLine=L.polyline(latLngs,{{color:'#2563eb',weight:5,opacity:0.7}}).addTo(map);map.fitBounds(routeLine.getBounds(),{{padding:[50,50]}});var distance=(data.routes[0].distance/1000).toFixed(1);var duration=Math.round(data.routes[0].duration/60);document.getElementById('routeInfo').innerHTML='📍 '+destName+'<br>📏 '+distance+' km<br>⏱️ '+duration+' phút';currentDestination={{lat:destLat,lng:destLng,name:destName}};document.getElementById('routeControls').style.display='block';return;}}}}catch(error){{console.log('OSRM error:',error);}}routeLine=L.polyline([[userPos.lat,userPos.lng],[destLat,destLng]],{{color:'#dc2626',weight:4,opacity:0.7,dashArray:'8,6'}}).addTo(map);map.fitBounds(routeLine.getBounds(),{{padding:[40,40]}});var estimatedTime=Math.round((parseFloat(straightDist)/30)*60);document.getElementById('routeInfo').innerHTML='📍 '+destName+'<br>📏 ~'+straightDist+' km<br>⏱️ ~'+estimatedTime+' phút<br>🔴 Offline';currentDestination={{lat:destLat,lng:destLng,name:destName}};document.getElementById('routeControls').style.display='block';}}
function drawOfflineRoute(destLat,destLng,destName){{if(offlineRouteElement)offlineRouteElement.remove();var container=document.getElementById('offline-map');var userX=parseFloat(offlineUserMarker.style.left);var userY=parseFloat(offlineUserMarker.style.top);var destX=window.offlineGetX(destLng);var destY=window.offlineGetY(destLat);var line=document.createElement('div');line.className='offline-route';var dx=destX-userX;var dy=destY-userY;var length=Math.sqrt(dx*dx+dy*dy);var angle=Math.atan2(dy,dx)*180/Math.PI;line.style.left=userX+'%';line.style.top=userY+'%';line.style.width=length+'%';line.style.transform='rotate('+angle+'deg)';container.appendChild(line);offlineRouteElement=line;var dist=calculateDistance({userLatStr},{userLngStr},destLat,destLng);var time=Math.round((dist/30)*60);document.getElementById('routeInfo').innerHTML='📍 '+destName+'<br>📏 ~'+dist.toFixed(1)+' km<br>⏱️ ~'+time+' phút<br>🔴 Offline';currentDestination={{lat:destLat,lng:destLng,name:destName}};document.getElementById('routeControls').style.display='block';}}
function calculateDistance(lat1,lng1,lat2,lng2){{var R=6371;var dLat=(lat2-lat1)*Math.PI/180;var dLng=(lng2-lng1)*Math.PI/180;var a=Math.sin(dLat/2)*Math.sin(dLat/2)+Math.cos(lat1*Math.PI/180)*Math.cos(lat2*Math.PI/180)*Math.sin(dLng/2)*Math.sin(dLng/2);return R*2*Math.atan2(Math.sqrt(a),Math.sqrt(1-a));}}
function startRoute(lat,lng,name){{console.log('Route to:',name);if(isOnline&&map){{drawOnlineRoute(lat,lng,name);map.closePopup();}}else{{drawOfflineRoute(lat,lng,name);}}}}
function stopRoute(){{if(routeLine&&map){{map.removeLayer(routeLine);routeLine=null;}}if(offlineRouteElement){{offlineRouteElement.remove();offlineRouteElement=null;}}currentDestination=null;document.getElementById('routeControls').style.display='none';}}
function switchToOffline(){{isOnline=false;updateStatus(false);initOfflineMap();}}
function switchToOnline(){{isOnline=true;updateStatus(true);initOnlineMap();}}
window.addEventListener('online',function(){{console.log('🟢 Online');switchToOnline();}});
window.addEventListener('offline',function(){{console.log('🔴 Offline');switchToOffline();}});
window.chrome.webview.addEventListener('message',function(e){{var data=JSON.parse(e.data);if(data.type==='updateUser'){{if(userMarker)userMarker.setLatLng([data.lat,data.lng]);if(offlineUserMarker){{offlineUserMarker.style.left=window.offlineGetX(data.lng)+'%';offlineUserMarker.style.top=window.offlineGetY(data.lat)+'%';}}}}}});
updateStatus(isOnline);if(isOnline){{initOnlineMap();}}else{{initOfflineMap();}}
console.log('✅ Ready');
</script>
</body>
</html>";
}
*/
