open Ll
open Args
// open Assert
open Backend
open Debug

(* configuration flags ------------------------------------------------------ *)

let clang = ref false

let assemble = ref true
let link = ref true
let executable_filename = ref "a.out"
let execute_x86 = ref false

let link_files = ref []
let add_link_file path = link_files := path :: (!link_files)

exception Ran_tests

// let suite =
//     ref (
//         Providedtests.provided_tests
//         @ Gradedtests.graded_tests
//     )

// let execute_tests () =
//     Platform.configure ()
//     let outcome = run_suite !suite
//     Printf.printf "%s\n" (outcome_to_string outcome)
//     raise Ran_tests

let process_ll_file (path: string) =
    //printfn "%A" (path)
    let file =
        System.IO.Path.GetFileNameWithoutExtension(path)

    let _ =
        Platform.verb
        <| Printf.sprintf "* processing file: %s\n" path in

    let ll_ast = Driver.parse_file path in

    let _ =
        if !print_ll then
            Driver.print_ll path ll_ast in

    let dot_s_file =
        Platform.gen_name !Platform.output_path file ".s" in

    let dot_o_file =
        Platform.gen_name !Platform.output_path file ".o" in

    let _ =
        if !clang then
            (Platform.verb "* compiling with clang"
             Platform.clang_compile path dot_s_file
             Driver.print_banner dot_s_file

             if !print_x86 then
                 Platform.sh (Printf.sprintf "cat %s" dot_s_file) Platform.raise_error)
        else
            (let asm_ast = Backend.compile_prog ll_ast in //后端编译入口
             let asm_str = X86.string_of_prog asm_ast in

             let _ =
                 if !print_x86 then
                     Driver.print_x86 dot_s_file asm_str in

             let _ = Driver.write_file dot_s_file asm_str in
             ())

    let _ =
        if !assemble then
            Platform.assemble dot_s_file dot_o_file in

    let _ = add_link_file dot_o_file in
    ()

let process_file (path: string) =
    let ext = System.IO.Path.GetExtension(path)
    // printfn "%A" ext
    (match ext with
     | ".ll" -> process_ll_file path
     | ".o" -> add_link_file path
     | ".c" -> add_link_file path
     | _ ->
         failwith
         <| Printf.sprintf "found unsupported file type: %s" path)

let process_files files =
    if (List.length files) > 0 then
        (List.iter process_file files

         (if !link then
              Platform.link (List.rev !link_files) !executable_filename)

         (if !execute_x86 then
              let ret =
                  Driver.run_executable "" !executable_filename in

              Driver.print_banner
              <| Printf.sprintf "Executing: %s" !executable_filename

              Printf.printf "* %s returned %d\n" !executable_filename ret))

let args =
    [ ("-linux", Set Platform.linux, "use linux-style name mangling [must preceed --test on linux]")
      //   ("--test", Unit execute_tests, "run the test suite, ignoring other files inputs")
      ("-op",
       String(fun s -> Platform.output_path := s),
       "set the path to the output files directory  [default='output']")
      ("-o", String(fun s -> executable_filename := s), "set the name of the resulting executable [default='a.out']")
      ("-S", Clear assemble, "stop after generating .s files; do generate .o files")
      ("-c", Clear link, "stop after generating .o files; do not generate executables")
      ("-g", Set debug, "debug mode show token ast ll x86")
      ("--print-ll",
       Set print_ll,
       "prints the program's LL code (after lowering to clang code if --clang-malloc is set)")
      ("--clang", Set clang, "compiles to assembly using clang, not the 341 backend (implies --clang-malloc)")
      ("--print-x86", Set print_x86, "prints the program's assembly code")
      ("--execute-x86", Set execute_x86, "run the resulting executable file")
      ("-v", Set Platform.verbose, "enables more verbose compilation output") ]

let files = ref []

// let _ =
//   try
let argsinfo =
    Array.ofList <| List.map Args.ArgInfo.Create args

let _ =
    ArgParser.Parse(argsinfo, (fun filename -> files := filename :: !files))
// ArgParser.Parse argsinfo  //TODO


if !debug then
    print_token := true
    print_ast := true
    print_ll := true
    print_x86 := true
else
    ()

let _ = Platform.configure ()

if !debug then
    printfn "%A" !files
else
    ()

process_files !files

let usage = "USAGE: dotnet run [-g] <files>\n\
  see ReadME.md for details about using the compiler"

if (!files).Length = 0 then
    printfn "%s" usage
else
    ()



// with Ran_tests ->
//()
