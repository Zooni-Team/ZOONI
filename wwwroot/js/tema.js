document.addEventListener('DOMContentLoaded', () => {
    const switchEl = document.querySelector('#temaSwitch');
    if(switchEl){
      switchEl.addEventListener('change', () => {
        const modo = switchEl.checked ? 'oscuro' : 'claro';
        document.body.classList.remove('tema-claro', 'tema-oscuro');
        document.body.classList.add(`tema-${modo}`);
        fetch('/Home/CambiarTema', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ modo })
        }).then(() => {
          // opcional: recargar o no
        });
      });
    }
  });
  