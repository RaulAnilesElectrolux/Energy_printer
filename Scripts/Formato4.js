function toNumber(value) {
    if (value === null || value === undefined) return 0;
    return Number(String(value).replace(/[^0-9.-]/g, '')) || 0;
}

function normalizarConfigMargenes() {
    if (!window.configMargenes) return [];

    if (Array.isArray(window.configMargenes)) {
        return window.configMargenes;
    }

    return [window.configMargenes];
}

function aplicarMargenesDesdeBD() {
    const configuraciones = normalizarConfigMargenes();

    configuraciones.forEach(config => {
        if (!config || !config.LABEL_TYPE) return;

        const unidad = config.UNIT && config.UNIT.trim() !== '' ? config.UNIT : 'cm';
        const tipoEtiqueta = String(config.LABEL_TYPE).toUpperCase();

        if (tipoEtiqueta === 'USA') {
            document.documentElement.style.setProperty('--usa-marg-left-1', config.MARG_LEFT_1 + unidad);
            document.documentElement.style.setProperty('--usa-marg-right-1', config.MARG_RIGHT_1 + unidad);
            document.documentElement.style.setProperty('--usa-marg-top-1', config.MARG_TOP_1 + unidad);
            document.documentElement.style.setProperty('--usa-marg-bottom-1', config.MARG_BOTTOM_1 + unidad);

            document.documentElement.style.setProperty('--usa-marg-left-2', config.MARG_LEFT_2 + unidad);
            document.documentElement.style.setProperty('--usa-marg-right-2', config.MARG_RIGHT_2 + unidad);
            document.documentElement.style.setProperty('--usa-marg-top-2', config.MARG_TOP_2 + unidad);
            document.documentElement.style.setProperty('--usa-marg-bottom-2', config.MARG_BOTTOM_2 + unidad);
        }

        if (tipoEtiqueta === 'CANADA') {
            document.documentElement.style.setProperty('--canada-marg-left-1', config.MARG_LEFT_1 + unidad);
            document.documentElement.style.setProperty('--canada-marg-right-1', config.MARG_RIGHT_1 + unidad);
            document.documentElement.style.setProperty('--canada-marg-top-1', config.MARG_TOP_1 + unidad);
            document.documentElement.style.setProperty('--canada-marg-bottom-1', config.MARG_BOTTOM_1 + unidad);

            document.documentElement.style.setProperty('--canada-marg-left-2', config.MARG_LEFT_2 + unidad);
            document.documentElement.style.setProperty('--canada-marg-right-2', config.MARG_RIGHT_2 + unidad);
            document.documentElement.style.setProperty('--canada-marg-top-2', config.MARG_TOP_2 + unidad);
            document.documentElement.style.setProperty('--canada-marg-bottom-2', config.MARG_BOTTOM_2 + unidad);
        }
    });
}

function renderEtiquetasAmarillas(pagina) {
    const d = window.datosAmarillo;

    const refType = pagina.querySelector('.display-ref-type');
    const defrost = pagina.querySelector('.display-defrost');
    const doorType = pagina.querySelector('.display-doertype');
    const ice = pagina.querySelector('.display-ice');
    const marca = pagina.querySelector('.display-marca');
    const modelo = pagina.querySelector('.display-modelo');
    const capacidad = pagina.querySelector('.display-capacidad');
    const costo = pagina.querySelector('.display-costo');
    const minAll = pagina.querySelector('.display-min-all');
    const maxAll = pagina.querySelector('.display-max-all');
    const minSim = pagina.querySelector('.display-min-sim');
    const maxSim = pagina.querySelector('.display-max-sim');
    const kwh = pagina.querySelector('.display-kwh');
    const partno = pagina.querySelector('.display-partno');

    if (refType) refType.innerText = d.REF_TYPE;
    if (defrost) defrost.innerText = d.DEFROST_SYSTEM;
    if (doorType) doorType.innerText = d.DOORTYPE;
    if (ice) ice.innerText = d.ICE_SERVICE;
    if (marca) marca.innerText = d.CUST_NAME;
    if (modelo) modelo.innerText = d.MODEL;
    if (capacidad) capacidad.innerText = d.CAB_SIZE;
    if (costo) costo.innerText = d.ENERGY_COST;
    if (minAll) minAll.innerText = d.LOW_AMOUNT;
    if (maxAll) maxAll.innerText = d.HIGH_AMOUNT;
    if (minSim) minSim.innerText = d.LOW_SIMILAR_MODEL;
    if (maxSim) maxSim.innerText = d.HIGH_SIMILAR_MODEL;
    if (kwh) kwh.innerText = d.ELECTRICITY_USE;
    if (partno) partno.innerText = 'PART NO. ' + d.PART_NUMBER;

    const logo = pagina.querySelector('.logo-energy-star');

    if (logo) {
        logo.style.visibility = (d.ENERGY_LOGO === 'Y' || d.ENERGY_LOGO === 'y') ? 'visible' : 'hidden';
    }

    const lowAmount = toNumber(d.LOW_AMOUNT);
    const highAmount = toNumber(d.HIGH_AMOUNT);
    const lowSimilar = toNumber(d.LOW_SIMILAR_MODEL);
    const highSimilar = toNumber(d.HIGH_SIMILAR_MODEL);
    const energyCost = toNumber(d.ENERGY_COST);

    const globalMin = Math.min(lowAmount, lowSimilar);
    const globalMax = Math.max(highAmount, highSimilar);
    const rango = globalMax - globalMin;

    const pct = v => rango === 0 ? 0 : ((v - globalMin) / rango) * 100;
    const clamp = v => Math.max(0, Math.min(100, v));

    const barraAll = pagina.querySelector('.barra-all');

    if (barraAll) {
        const leftAll = clamp(pct(lowAmount));
        const rightAll = clamp(pct(highAmount));

        barraAll.style.left = leftAll + '%';
        barraAll.style.width = Math.max(0, rightAll - leftAll) + '%';
    }

    const barraSim = pagina.querySelector('.barra-similar');

    if (barraSim) {
        const leftSim = clamp(pct(lowSimilar));
        const rightSim = clamp(pct(highSimilar));

        barraSim.style.left = leftSim + '%';
        barraSim.style.width = Math.max(0, rightSim - leftSim) + '%';
    }

    const separador = pagina.querySelector('.separador-sim');

    if (separador) {
        separador.style.left = clamp(pct(lowSimilar)) + '%';
    }

    const indicador = pagina.querySelector('.indicador-maestro');
    const contenedorIndicador = pagina.querySelector('.cost-scale-align');
    const barraReal = pagina.querySelector('.track-outline');
    const costBox = pagina.querySelector('.cost-box');

    if (indicador && contenedorIndicador && barraReal && costBox) {
        const posicionReal = pct(energyCost);
        const posicionVisual = clamp(posicionReal);

        const barraRect = barraReal.getBoundingClientRect();
        const contenedorRect = contenedorIndicador.getBoundingClientRect();

        const xBarra = barraRect.left - contenedorRect.left;
        const anchoBarra = barraRect.width;

        const posicionPx = xBarra + ((posicionVisual / 100) * anchoBarra);

        indicador.style.left = posicionPx + 'px';
        indicador.style.transform = 'none';

        const valorCosto = indicador.querySelector('.cost-value');

        if (valorCosto) {
            valorCosto.style.transform = 'translateX(-50%)';

            requestAnimationFrame(() => {
                const valorRect = valorCosto.getBoundingClientRect();
                const cajaRect = costBox.getBoundingClientRect();

                let ajustePx = 0;

                if (valorRect.left < cajaRect.left) {
                    ajustePx = cajaRect.left - valorRect.left + 4;
                }

                if (valorRect.right > cajaRect.right) {
                    ajustePx = cajaRect.right - valorRect.right - 4;
                }

                valorCosto.style.transform = `translateX(calc(-50% + ${ajustePx}px))`;
            });
        }
    }
}

function renderEtiquetasBlancas(contenedor) {
    const d = window.datosBlanco;

    const elModelKw = contenedor.querySelector('.val-model-kw');
    if (elModelKw) elModelKw.textContent = d.MODEL_KW;

    const elLowKw = contenedor.querySelector('.val-low-kw');
    if (elLowKw) elLowKw.textContent = d.LOW_KW;

    const elHighKw = contenedor.querySelector('.val-high-kw');
    if (elHighKw) elHighKw.textContent = d.HIGH_KW;

    const elType = contenedor.querySelector('.val-type');
    if (elType) elType.textContent = d.TYPE;

    const elRange = contenedor.querySelector('.val-range');
    if (elRange) elRange.textContent = d.RANGE;

    const elModel = contenedor.querySelector('.val-model');
    if (elModel) elModel.textContent = d.MODEL;

    const lowKw = toNumber(d.LOW_KW);
    const highKw = toNumber(d.HIGH_KW);
    const modelKw = toNumber(d.MODEL_KW);

    const rango = highKw - lowKw;

    let porcentaje = 0;

    if (rango !== 0) {
        porcentaje = (100 * (modelKw - lowKw)) / rango;
    }

    const menorAlMinimo = modelKw < lowKw;

    const arrow = contenedor.querySelector('.arrow');
    const modelArrow = contenedor.querySelector('.model-arrow');
    const scaleEl = contenedor.querySelector('.scale');

    if (arrow && modelArrow && scaleEl) {
        if (menorAlMinimo) {
            const scaleRect = scaleEl.getBoundingClientRect();
            const arrowContainerRect = modelArrow.getBoundingClientRect();

            const distanciaHastaEscala = scaleRect.left - arrowContainerRect.left;
            const margenFlecha = distanciaHastaEscala;

            arrow.style.marginLeft = Math.max(0, margenFlecha) + 'px';

            modelArrow.style.flexDirection = 'row';
            modelArrow.style.justifyContent = 'flex-start';
        } else {
            const pctClamped = Math.max(0, Math.min(100, porcentaje));

            arrow.style.marginLeft = pctClamped + '%';

            if (porcentaje > 90) {
                modelArrow.style.flexDirection = 'row-reverse';
                modelArrow.style.justifyContent = 'flex-end';
            } else {
                modelArrow.style.flexDirection = 'row';
                modelArrow.style.justifyContent = 'flex-start';
            }
        }
    }

    const energyLogo = contenedor.querySelector('.bottom');

    if (energyLogo && d.ENERGY_LOGO === "N") {
        energyLogo.style.display = "none";
    }
}

window.onload = function () {
    aplicarMargenesDesdeBD();

    const contenedoresAmarillos = document.querySelectorAll('.page-amarilla .usa-container');
    contenedoresAmarillos.forEach(renderEtiquetasAmarillas);

    const contenedoresBlancos = document.querySelectorAll('.page-blanca .canada-container');
    contenedoresBlancos.forEach(renderEtiquetasBlancas);
};