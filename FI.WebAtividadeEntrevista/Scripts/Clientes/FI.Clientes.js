
$(document).ready(function () {

    let beneficiariosTemporarios = [];

    // Função para validar o CPF
    function validarCPF(cpf) {
        const cpfLimpo = cpf.replace(/\D/g, '');

        // Verifica se o CPF tem 11 dígitos e não é uma sequência repetida
        if (cpfLimpo.length !== 11 || /^(\d)\1+$/.test(cpfLimpo)) {
            return false;
        }

        // Função para calcular o dígito verificador
        function calcularDigito(cpf, pesoInicial) {
            let soma = 0;
            for (let i = 0; i < pesoInicial - 1; i++) {
                soma += parseInt(cpf[i]) * (pesoInicial - i);
            }
            const resto = (soma * 10) % 11;
            return resto === 10 ? 0 : resto;
        }

        // Calcula o primeiro e segundo dígitos verificadores
        const digito1 = calcularDigito(cpfLimpo, 10);
        const digito2 = calcularDigito(cpfLimpo, 11);

        // Verifica se os dígitos calculados conferem com os dígitos finais do CPF
        return digito1 === parseInt(cpfLimpo[9]) && digito2 === parseInt(cpfLimpo[10]);
    }

    $('#formCadastro').submit(function (e) {
        e.preventDefault();

        var $form = $(this); // Save reference to the form

        const cpf = $form.find("#CPF").val();

        const formData = {
            "NOME": $form.find("#Nome").val(),
            "CEP": $form.find("#CEP").val(),
            "Email": $form.find("#Email").val(),
            "Sobrenome": $form.find("#Sobrenome").val(),
            "Nacionalidade": $form.find("#Nacionalidade").val(),
            "Estado": $form.find("#Estado").val(),
            "Cidade": $form.find("#Cidade").val(),
            "Logradouro": $form.find("#Logradouro").val(),
            "Telefone": $form.find("#Telefone").val(),
            "CPF": $form.find("#CPF").val(),
            "beneficiariosData": $form.find("#beneficiariosData").val()
        };

        // Valida o CPF antes de enviar o formulário
        if (!validarCPF(cpf)) {
            ModalDialog("Ocorreu um erro:", "CPF Inválido!");
            return;
        }

        // Verifica se o CPF já está cadastrado
        $.ajax({
            url: '/Cliente/VerificarCPF',
            method: 'POST',
            data: { "CPF": $form.find("#CPF").val() },
            success: function (response) {
                if (response.existe) {
                    ModalDialog("Ocorreu um erro", "CPF já cadastrado!");
                } else {
                    $.ajax({
                        url: urlPost,
                        method: "POST",
                        data: formData,
                        error: function (r) {
                            if (r.status == 400)
                                ModalDialog("Ocorreu um erro", r.responseJSON);
                            else if (r.status == 500)
                                ModalDialog("Ocorreu um erro", "Ocorreu um erro interno no servidor.");
                        },
                        success: function (r) {
                            ModalDialog("Sucesso!", r)
                            $form[0].reset();
                        }
                    });
                }
            },
            error: function () {
                ModalDialog("Ocorreu um erro", "Erro ao verificar o CPF.");
            }
        });
    });

    $("#incluirBeneficiario").on("click", function () {
        const cpf = $("#beneficiarioCPF").val();
        const nome = $("#beneficiarioNome").val();

        // Verifica se o CPF e o Nome foram preenchidos
        if (!cpf || !nome) {
            alert("Por favor, preencha ambos os campos CPF e Nome.");
            return;
        }

        // Valida o CPF antes de enviar o formulário
        if (!validarCPF(cpf)) {
            // Exibe uma mensagem de erro se o CPF for inválido
            ModalDialog("Ocorreu um erro:", "CPF Inválido!");
            return; // Interrompe o envio
        }

        $.ajax({
            url: '/Cliente/VerificarCpfBeneficiario',
            method: 'POST',
            data: { "CPF": cpf },
            success:
                function (response) {
                if (response.existe) {
                    ModalDialog("Ocorreu um erro", "CPF já cadastrado!");
                }
                else {
                    // Cria um objeto para o beneficiário e adiciona à lista temporária
                    const novoBeneficiario = {
                        CPF: cpf,
                        Nome: nome
                    };
                    beneficiariosTemporarios.push(novoBeneficiario);

                    // Adiciona o novo beneficiário na tabela do modal
                    $("#beneficiariosTable tbody").append(
                        `<tr>
                            <td>${novoBeneficiario.CPF}</td>
                            <td>${novoBeneficiario.Nome}</td>
                            <td>
                                <button id="ModalAlterar" class="btn btn-primary btn-alterar" data-cpf="${novoBeneficiario.CPF}">Alterar</button>
                                <button id="ModalExcluir" class="btn btn-danger btn-excluir" data-cpf="${novoBeneficiario.CPF}">Excluir</button>
                            </td>
                        </tr>`
                    );

                    // Limpa os campos de entrada no modal
                    $("#beneficiarioCPF").val("");
                    $("#beneficiarioNome").val("");
                }
            },
            error: function () {
                ModalDialog("Ocorreu um erro", "Erro ao verificar o CPF.");
            }
        });        
    });

    // Atualiza o campo oculto com os dados dos beneficiários temporários ao fechar o modal
    $('#beneficiariosModal').on('hidden.bs.modal', function () {
        // Converte o array para JSON e define no campo oculto
        $('#beneficiariosData').val(JSON.stringify(beneficiariosTemporarios));
    });


    // Manipulador de evento para excluir um beneficiário
    $("#beneficiariosTable").on("click", "#ModalExcluir", function () {
        const cpf = $(this).data("cpf");
        const linha = $(this).closest("tr");

        // Confirmação de exclusão
        if (confirm("Tem certeza que deseja excluir este beneficiário?")) {
           
            //Remove o beneficiário do array temporário pelo CPF
            beneficiariosTemporarios = beneficiariosTemporarios.filter(b => b.CPF !== cpf);

            // Atualiza o campo oculto com os dados atualizados
            $('#beneficiariosData').val(JSON.stringify(beneficiariosTemporarios));

            // Remove a linha da tabela
            linha.remove();

            alert("Beneficiário excluído com sucesso.");
        }
    });

    // Evento de clique no botão de Alterar
    $("#beneficiariosTable").on("click", "#ModalAlterar", function () {
        const cpf = $(this).data("cpf"); // Obtém o CPF do beneficiário
        const linha = $(this).closest("tr"); // A linha da tabela a ser alterada

        // Encontra o beneficiário pelo CPF no array temporário
        const beneficiario = beneficiariosTemporarios.find(b => b.CPF === cpf);
        if (!beneficiario) {
            alert("Beneficiário não encontrado.");
            return;
        }

        // Exibe um prompt para alterar o nome do beneficiário
        const novoNome = prompt("Digite o novo nome para o beneficiário:", beneficiario.Nome);

        // Se o usuário cancelar ou não digitar nada, sai da função
        if (novoNome === null || novoNome.trim() === "") {
            alert("Alteração cancelada.");
            return;
        }

        // Atualiza o nome do beneficiário no array temporário
        beneficiario.Nome = novoNome;

        // Atualiza o campo oculto com os dados dos beneficiários temporários atualizados
        $('#beneficiariosData').val(JSON.stringify(beneficiariosTemporarios));

        // Atualiza o nome na linha correspondente da tabela
        linha.find("td:nth-child(2)").text(novoNome);

        alert("Nome do beneficiário alterado com sucesso.");
    });
})

function ModalDialog(titulo, texto) {
    var random = Math.random().toString().replace('.', '');
    var texto = '<div id="' + random + '" class="modal fade">                                                               ' +
        '        <div class="modal-dialog">                                                                                 ' +
        '            <div class="modal-content">                                                                            ' +
        '                <div class="modal-header">                                                                         ' +
        '                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>         ' +
        '                    <h4 class="modal-title">' + titulo + '</h4>                                                    ' +
        '                </div>                                                                                             ' +
        '                <div class="modal-body">                                                                           ' +
        '                    <p>' + texto + '</p>                                                                           ' +
        '                </div>                                                                                             ' +
        '                <div class="modal-footer">                                                                         ' +
        '                    <button type="button" class="btn btn-default" data-dismiss="modal">Fechar</button>             ' +
        '                                                                                                                   ' +
        '                </div>                                                                                             ' +
        '            </div><!-- /.modal-content -->                                                                         ' +
        '  </div><!-- /.modal-dialog -->                                                                                    ' +
        '</div> <!-- /.modal -->                                                                                        ';

    $('body').append(texto);
    $('#' + random).modal('show');
}
