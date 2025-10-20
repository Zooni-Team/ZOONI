// wwwroot/js/registro.js
(function(){
    const state = {
      step: 1,
      species: null,
      pet: { name:'', breed:'', color:'Gris', sex:'Macho', age:2, sterilizado:false, chip:'', peso:0 },
      user: { nombre:'', apellido:'', email:'', pass:'', pais:'', ciudad:'', telefono:'', edad:25 },
      perfil: { foto:'', descripcion:'' }
    };
  
    const screens = Array.from(document.querySelectorAll('.screen'));
    const showStep = (n) => {
      state.step = n;
      screens.forEach(s => s.classList.toggle('active', Number(s.dataset.step) === n));
      renderAll();
    };
  
    // navigation
    document.addEventListener('click', (e) => {
      const a = e.target;
      if(a.id === 'btnCrear') { showStep(2); }
      if(a.dataset.action === 'next') {
        if(state.step === 2 && !state.species) { alert('Seleccioná una especie'); return; }
        showStep(Math.min(6, state.step+1));
      }
      if(a.dataset.action === 'back') showStep(Math.max(1, state.step-1));
    });
  
    // species selection
    document.querySelectorAll('.species').forEach(btn => {
      btn.addEventListener('click', () => {
        document.querySelectorAll('.species').forEach(b=>b.classList.remove('active'));
        btn.classList.add('active');
        state.species = btn.dataset.species;
        renderAll();
      });
    });
  
    // bind inputs
    const bind = () => {
      const q = s => document.querySelector(s);
      if(q('#petName')) q('#petName').addEventListener('input', e=> state.pet.name = e.target.value);
      if(q('#petBreed')) q('#petBreed').addEventListener('input', e=> state.pet.breed = e.target.value);
      if(q('#petColor')) q('#petColor').addEventListener('change', e=> state.pet.color = e.target.value);
      if(q('#petSex')) q('#petSex').addEventListener('change', e=> state.pet.sex = e.target.value);
      if(q('#petAge')) q('#petAge').addEventListener('input', e=> { state.pet.age = +e.target.value; q('#ageValue').textContent = e.target.value; });
      if(q('#petSterilizado')) q('#petSterilizado').addEventListener('change', e=> state.pet.sterilizado = e.target.checked);
      if(q('#petChip')) q('#petChip').addEventListener('input', e=> state.pet.chip = e.target.value);
  
      if(q('#userNombre')) q('#userNombre').addEventListener('input', e=> state.user.nombre = e.target.value);
      if(q('#userApellido')) q('#userApellido').addEventListener('input', e=> state.user.apellido = e.target.value);
      if(q('#userEmail')) q('#userEmail').addEventListener('input', e=> state.user.email = e.target.value);
      if(q('#userPass')) q('#userPass').addEventListener('input', e=> state.user.pass = e.target.value);
      if(q('#userPais')) q('#userPais').addEventListener('input', e=> state.user.pais = e.target.value);
      if(q('#userCiudad')) q('#userCiudad').addEventListener('input', e=> state.user.ciudad = e.target.value);
      if(q('#userTelefono')) q('#userTelefono').addEventListener('input', e=> state.user.telefono = e.target.value);
      if(q('#userEdad')) q('#userEdad').addEventListener('input', e=> { state.user.edad = +e.target.value; q('#userAgeVal').textContent = e.target.value; });
  
      const changeAnimalBtn = document.getElementById('changeAnimal');
      if(changeAnimalBtn) changeAnimalBtn.addEventListener('click', ()=> showStep(2));
  
      const confirmBtn = document.getElementById('confirmRegister');
      if(confirmBtn) confirmBtn.addEventListener('click', submitRegister);
    };
  
    // SVG avatar generator (basic)
    const renderAvatarSVG = (species, color, breed, size=140) => {
      const fill = { 'Gris':'#cfcfcf','Blanco':'#fff','Negro':'#333','Naranja':'#f3b58a','Marrón':'#b07a4b' }[color] || '#cfcfcf';
      if(!species) species='Gato';
      if(species==='Gato'){
        return `<svg viewBox="0 0 120 120" width="${size}" height="${size}" xmlns="http://www.w3.org/2000/svg">
          <ellipse cx="60" cy="78" rx="36" ry="24" fill="${fill}" stroke="#a07a4c" stroke-width="2"/>
          <circle cx="45" cy="60" r="18" fill="${fill}" stroke="#a07a4c" stroke-width="2"/>
          <circle cx="75" cy="60" r="18" fill="${fill}" stroke="#a07a4c" stroke-width="2"/>
          <circle cx="50" cy="56" r="3" fill="#222" />
          <circle cx="70" cy="56" r="3" fill="#222" />
        </svg>`;
      }
      if(species==='Perro'){
        return `<svg viewBox="0 0 120 120" width="${size}" height="${size}" xmlns="http://www.w3.org/2000/svg">
          <ellipse cx="60" cy="72" rx="36" ry="30" fill="${fill}" stroke="#a07a4c" stroke-width="2"/>
          <circle cx="42" cy="54" r="14" fill="${fill}" stroke="#a07a4c" stroke-width="2"/>
          <circle cx="78" cy="54" r="14" fill="${fill}" stroke="#a07a4c" stroke-width="2"/>
          <circle cx="52" cy="56" r="3" fill="#222" />
          <circle cx="68" cy="56" r="3" fill="#222" />
        </svg>`;
      }
      // others simplified
      return `<svg viewBox="0 0 120 120" width="${size}" height="${size}" xmlns="http://www.w3.org/2000/svg"><circle cx="60" cy="60" r="36" fill="${fill}" /></svg>`;
    };
  
    const q = s => document.querySelector(s);
  
    const renderAll = () => {
      // species highlight
      document.querySelectorAll('.species').forEach(b => b.classList.toggle('active', b.dataset.species === state.species));
  
      // set pet inputs
      if(q('#petName')) q('#petName').value = state.pet.name;
      if(q('#petBreed')) q('#petBreed').value = state.pet.breed;
      if(q('#petColor')) q('#petColor').value = state.pet.color;
      if(q('#petSex')) q('#petSex').value = state.pet.sex;
      if(q('#petAge')) q('#petAge').value = state.pet.age;
      if(q('#ageValue')) q('#ageValue').textContent = state.pet.age;
      if(q('#petSterilizado')) q('#petSterilizado').checked = state.pet.sterilizado;
      if(q('#petChip')) q('#petChip').value = state.pet.chip;
  
      // user inputs
      if(q('#userNombre')) q('#userNombre').value = state.user.nombre;
      if(q('#userApellido')) q('#userApellido').value = state.user.apellido;
      if(q('#userEmail')) q('#userEmail').value = state.user.email;
      if(q('#userPass')) q('#userPass').value = state.user.pass;
      if(q('#userPais')) q('#userPais').value = state.user.pais;
      if(q('#userCiudad')) q('#userCiudad').value = state.user.ciudad;
      if(q('#userTelefono')) q('#userTelefono').value = state.user.telefono;
      if(q('#userEdad')) q('#userEdad').value = state.user.edad;
      if(q('#userAgeVal')) q('#userAgeVal').textContent = state.user.edad;
  
      // avatar render
      if(q('#petAvatar')) q('#petAvatar').innerHTML = renderAvatarSVG(state.species, state.pet.color, state.pet.breed, 140);
      if(q('#previewAvatar')) q('#previewAvatar').innerHTML = renderAvatarSVG(state.species, state.pet.color, state.pet.breed, 160);
      if(q('#successAvatar')) q('#successAvatar').innerHTML = renderAvatarSVG(state.species, state.pet.color, state.pet.breed, 160);
  
      // preview text
      if(q('#previewTitle')) q('#previewTitle').textContent = state.pet.name || (state.species || 'Mascota');
      if(q('#previewOwner')) q('#previewOwner').textContent = (state.user.nombre ? `${state.user.nombre} ${state.user.apellido}` : '—');
      if(q('#previewLocation')) q('#previewLocation').textContent = `${state.user.ciudad || '—'} / ${state.user.pais || '—'}`;
      if(q('#previewPetAge')) q('#previewPetAge').textContent = `${state.pet.age || '—'} años`;
      if(q('#previewBreedColor')) q('#previewBreedColor').textContent = `${state.pet.breed || '—'} • ${state.pet.color || '—'}`;
  
      // title on pet details
      if(q('#petTitle')) q('#petTitle').textContent = state.species ? `Ingrese los datos de su ${state.species.toLowerCase()}` : 'Ingrese los datos de su mascota';
    };
  
    // submit the full registration
    async function submitRegister(){
      // minimal validation
      if(!state.user.email || !state.user.pass) { alert('Completá email y contraseña'); showStep(4); return; }
      if(!state.pet.name) { alert('Completá el nombre de la mascota'); showStep(3); return; }
  
      const dto = {
        Email: state.user.email,
        Password: state.user.pass,
        Nombre: state.user.nombre,
        Apellido: state.user.apellido,
        Pais: state.user.pais,
        Ciudad: state.user.ciudad,
        Telefono: state.user.telefono,
        EdadUsuario: state.user.edad,
        FotoPerfil: state.perfil.foto,
        DescripcionPerfil: state.perfil.descripcion,
        PetNombre: state.pet.name,
        PetEspecie: state.species,
        PetRaza: state.pet.breed,
        PetColor: state.pet.color,
        PetEdad: state.pet.age,
        PetEsterilizado: state.pet.sterilizado,
        PetChip: state.pet.chip,
        PetPeso: state.pet.peso
      };
  
      try {
        const res = await fetch('/Account/RegisterFull', {
          method: 'POST',
          headers: {
            'Content-Type':'application/json',
            'RequestVerificationToken': window.AntiForgeryToken || ''
          },
          body: JSON.stringify(dto)
        });
        const data = await res.json();
        if(data.ok){
          showStep(6);
        } else {
          alert('Error: ' + (data.error || 'No se pudo registrar'));
        }
      } catch(err){
        console.error(err);
        alert('Error al comunicarse con el servidor.');
      }
    }
  
    // init
    bind();
    renderAll();
    showStep(1);
  })();
  